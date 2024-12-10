using Babble.Core.Enums;
using Babble.Core.Scripts;
using Babble.Core.Scripts.Config;
using Babble.Core.Scripts.Captures;
using Babble.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json;
using NReco.Logging.File;
using OpenCvSharp;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Babble.Core;

/// <summary>
/// The singleton entrypoint for our library
/// </summary>
public partial class BabbleCore
{
    [MemberNotNullWhen(true, nameof(PlatformConnector), nameof(_session), nameof(_floatFilter), nameof(_calibrationItems))]
    public bool IsRunning { get; private set; }
    public int FPS => (int)MathF.Floor(1000f / MS);
    public float MS { get; private set; }
    public static BabbleCore Instance { get; private set; }
    public BabbleSettings Settings { get; private set; }
    public ILogger<BabbleCore> Logger { get; private set; }

    public PlatformConnector? PlatformConnector;
    
    private readonly DenseTensor<float> _inputTensor = new DenseTensor<float>([1, 1, 256, 256]);
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private Size _inputSize = new Size(256, 256);
    private Dictionary<string, CalibrationItem>? _calibrationItems;
    private InferenceSession? _session;
    private OneEuroFilter? _floatFilter;
    private float _lastTime = 0;
    private string? _inputName;
    
    static BabbleCore()
    {
        Instance = new BabbleCore();
    }

    private BabbleCore()
    {
        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ProjectBabble");

        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }

        ServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging((loggingBuilder) => loggingBuilder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug)
                .AddFile(Path.Combine(logPath, "latest.log"), append: false))
            .BuildServiceProvider();
        Logger = serviceProvider.GetService<ILoggerFactory>()!.CreateLogger<BabbleCore>();

        Settings = new BabbleSettings();
        Settings.OnUpdate += (setting) =>
        {
            var captureSource = nameof(Settings.Cam.CaptureSource);
            var calibArray = nameof(Settings.GeneralSettings.CalibArray);

            if (setting == captureSource)
            {
                if (PlatformConnector is not null)
                    PlatformConnector!.Terminate();
                ConfigurePlatformConnector();
            }

            if (setting == calibArray)
            {
                _calibrationItems = JsonConvert.
                    DeserializeObject<CalibrationItem[]>(Instance.Settings.GeneralSettings.CalibArray)!.
                    ToDictionary(x => x.ShapeName);
            }
        };
    }
    
    /// <summary>
    /// Big bang. True to load saved settings, false to use default ones.
    /// </summary>
    /// <param name="loadConfig">Load saved config, else supply defaults.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Start(bool loadConfig = true)
    {
        if (loadConfig)
        {
            Instance.Settings.Load();
            Start(Instance.Settings);
        }
        else
        {
            Start(new BabbleSettings());
        }
    }

    /// <summary>
    /// Big bang, but with custom BabbleSettings.
    /// </summary>
    /// <param name="settings"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Start(BabbleSettings settings)
    {
        // Fail fast
        if (IsRunning)
        {
            Logger.LogInformation("Babble.Core was already running. Restarting...");
            Stop();
        }

        Settings = settings;
        _calibrationItems = JsonConvert.
            DeserializeObject<CalibrationItem[]>(Instance.Settings.GeneralSettings.CalibArray)!.
            ToDictionary(x => x.ShapeName);

        // Model manifest on Github?
        const string defaultModelName = "model.onnx";
        string modelPath = Path.Combine(AppContext.BaseDirectory, defaultModelName);
        Utils.ExtractEmbeddedResource(
            Assembly.GetExecutingAssembly(), 
            Assembly.
                GetExecutingAssembly().
                GetManifestResourceNames().
                First(x => x.Contains(defaultModelName)), // Babble model
            modelPath, 
            overwrite: false);

        if (File.Exists(Instance.Settings.GeneralSettings.GuiModelFile))
        {
            modelPath = Instance.Settings.GeneralSettings.GuiModelFile;
        }

        SessionOptions sessionOptions = SetupSessionOptions();
        ConfigurePlatformSpecificGPU(sessionOptions);

        ConfigurePlatformConnector();

        var minCutoff = settings.GeneralSettings.GuiMinCutoff > 0 ? settings.GeneralSettings.GuiMinCutoff : 1.0f;
        var speedCoeff = settings.GeneralSettings.GuiSpeedCoefficient > 0 ? settings.GeneralSettings.GuiSpeedCoefficient : 0.007f;
        _floatFilter = new OneEuroFilter(
            minCutoff: minCutoff,
            beta: speedCoeff
        );

        _session = new InferenceSession(modelPath, sessionOptions);
        _inputName = _session.InputMetadata.Keys.First().ToString();
        int[] dimensions = _session.InputMetadata.Values.First().Dimensions;
        _inputSize = new(dimensions[2], dimensions[3]);
        IsRunning = true;


        Logger.LogInformation("Babble.Core started.");
    }

    /// <summary>
    /// Poll expression data, frames
    /// </summary>
    /// <param name="UnifiedExpressions"></param>
    /// <returns></returns>
    public bool GetExpressionData(out Dictionary<UnifiedExpression, float>? UnifiedExpressions)
    {
        UnifiedExpressions = null;
        if (!IsRunning || Instance is null)
        {
            Logger.LogError("Tried to to poll Babble.Core, but it wasn't running!");
            return false;
        }
        
        // Test if the camera is not ready or connecting to new source
        if (!PlatformConnector.ExtractFrameData(_inputTensor.Buffer.Span, _inputSize)) return false;

        // Camera ready, prepare Mat as DenseTensor
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, _inputTensor)
        };

        // Run inference!
        using var results = _session.Run(inputs);
        var output = results[0].AsEnumerable<float>().ToArray();
        float time = (float)_sw.Elapsed.TotalSeconds;
        MS = (time - _lastTime) * 1000;

        // Clamp and give meaning to the floats
        var arKitExpressions = Utils.ARKitExpressions;
        for (int i = 0; i < output.Length; i++)
        {
            arKitExpressions[(ARKitExpression)i] = output[i];
        }

        // Map unfiltered ARKit expressions to filtered Unified Expressions
        // TODO Replace clamp with remap?
        int j = 0;
        foreach (var exp in Utils.ExpressionMapping)
        {
            float filteredValue = arKitExpressions[exp.Key];
            foreach (var ue in exp.Value)
            {
                var expressionName = BabbleAddresses.Addresses[ue];
                filteredValue = _floatFilter.Filter(filteredValue, time - _lastTime);
                CachedExpressionTable[ue] = Math.Clamp(filteredValue, _calibrationItems[expressionName].Min, _calibrationItems[expressionName].Max);
            }
            j++;
        }

        _lastTime = time;
        UnifiedExpressions = CachedExpressionTable;
        return true;
    }

    /// <summary>
    /// Gets the pre-transform lip image for this frame
    /// This image will be (dimensions.width)px * (dimensions.height)px in provided ColorType
    /// </summary>
    /// <param name="image"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public bool GetRawImage(ColorType color, out byte[] image, out (int width, int height) dimensions)
    {
        if (PlatformConnector?.Capture!.RawMat is null)
        {
            dimensions = (0, 0);
            image = Array.Empty<byte>();
            return false;
        }

        dimensions = PlatformConnector.Capture.Dimensions;
        if (color == ((PlatformConnector.Capture.RawMat.Channels() == 1) ? ColorType.GRAY_8 : ColorType.BGR_24))
        {
            image = PlatformConnector.Capture.RawMat.AsSpan<byte>().ToArray();
        }
        else
        {
            using var convertedMat = new Mat();
            Cv2.CvtColor(PlatformConnector.Capture.RawMat, convertedMat, (PlatformConnector.Capture.RawMat.Channels() == 1) ? color switch {
                ColorType.BGR_24 => ColorConversionCodes.GRAY2BGR,
                ColorType.RGB_24 => ColorConversionCodes.GRAY2RGB,
                ColorType.RGBA_32 => ColorConversionCodes.GRAY2RGBA,
            } : color switch {
                ColorType.GRAY_8 => ColorConversionCodes.BGR2GRAY,
                ColorType.RGB_24 => ColorConversionCodes.BGR2RGB,
                ColorType.RGBA_32 => ColorConversionCodes.BGR2RGBA,
            });
            image = convertedMat.AsSpan<byte>().ToArray();
        }

        return true;
    }

    /// <summary>
    /// Gets the prost-transform lip image for this frame
    /// This image will be 256*256px, single-channel
    /// </summary>
    /// <param name="image"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public unsafe bool GetImage(out byte[]? image, out (int width, int height) dimensions)
    {
        image = null;
        dimensions = (0, 0);

        byte[] data = new byte[_inputSize.Width * _inputSize.Height];
        using var imageMat = Mat<byte>.FromPixelData(_inputSize.Height, _inputSize.Width, data);
        if (PlatformConnector?.TransformRawImage(imageMat) != true) return false;

        image = data;
        dimensions = (imageMat.Width, imageMat.Height);
        return true;
    }

    /// <summary>
    /// Stop things
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
        {
            Logger.LogWarning("Tried to to stop Babble.Core, but it wasn't running!");
            return;
        }

        IsRunning = false;
        _session.Dispose();
        PlatformConnector.Terminate();
        Logger.LogInformation("Babble.Core stopped");
    }

    /// <summary>
    /// Make our SessionOptions *fancy*
    /// </summary>
    /// <returns></returns>
    private SessionOptions SetupSessionOptions()
    {
        // Random environment variable(s) to speed up webcam opening on the MSMF backend.
        // https://github.com/opencv/opencv/issues/17687
        Environment.SetEnvironmentVariable("OPENCV_VIDEOIO_MSMF_ENABLE_HW_TRANSFORMS", "0");
        Environment.SetEnvironmentVariable("OMP_NUM_THREADS", "1");

        // Setup inference backend
        var sessionOptions = new SessionOptions();
        sessionOptions.InterOpNumThreads = 1;
        sessionOptions.IntraOpNumThreads = Math.Clamp(Settings.GeneralSettings.GuiInferenceThreads, 0, 2);
        sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        // ~3% savings worth ~6ms avg latency. Not noticeable at 60fps?
        sessionOptions.AddSessionConfigEntry("session.intra_op.allow_spinning", "0");  
        sessionOptions.EnableMemoryPattern = true;
        return sessionOptions;
    }

    /// <summary>
    /// Creates the proper video streaming classes based on the platform we're deploying to.
    /// EmguCV doesn't have support for VideoCapture on Android, iOS, or UWP
    /// We have a custom implementations for IP Cameras, the de-facto use case on mobile
    /// As well as SerialCameras (not tested on mobile yet)
    /// </summary>
    private void ConfigurePlatformConnector()
    {
        // We should never change from Android to Desktop or vice versa during
        // the applications runtime. Only run these once!
        if (PlatformConnector is null)
        {
            if (OperatingSystem.IsAndroid())
            {
                PlatformConnector = new AndroidConnector(Settings.Cam.CaptureSource);
            } 
            else
            {
                // Else, for WinUI, macOS, watchOS, MacCatalyst, tvOS, Tizen, etc...
                // Use the standard EmguCV VideoCapture backend
                PlatformConnector = new DesktopConnector(Settings.Cam.CaptureSource);
            }
        }
        
        PlatformConnector.Initialize(Settings.Cam.CaptureSource);
    }

    /// <summary>
    /// Per-platform hardware accel. detection/activation
    /// </summary>
    /// <param name="sessionOptions"></param>
    private void ConfigurePlatformSpecificGPU(SessionOptions sessionOptions)
    {
        sessionOptions.AppendExecutionProvider_CPU();
        if (!Settings.GeneralSettings.GuiUseGpu)
        {
            return;
        }

        var gpuIndex = Settings.GeneralSettings.GuiGpuIndex;

        // "The Android Neural Networks API (NNAPI) is an Android C API designed for
        // running computationally intensive operations for machine learning on Android devices."
        // It was added in Android 8.1 and will be deprecated in Android 15
        if (OperatingSystem.IsAndroid() &&
            OperatingSystem.IsAndroidVersionAtLeast(8, 1) && // At least 8.1
            !OperatingSystem.IsAndroidVersionAtLeast(15))          // At most 15
        {
            sessionOptions.AppendExecutionProvider_Nnapi();
        }
        else if (OperatingSystem.IsIOS() ||
                 OperatingSystem.IsMacCatalyst() ||
                 OperatingSystem.IsMacOS() ||
                 OperatingSystem.IsWatchOS() ||
                 OperatingSystem.IsTvOS())
        {
            sessionOptions.AppendExecutionProvider_CoreML();
        }
        else if (OperatingSystem.IsWindows())
        {
            // If DirectML is supported on the user's system, try using it first.
            // This has support for both AMD and Nvidia GPUs, and uses less memory in my testing
            try
            {
                sessionOptions.AppendExecutionProvider_DML(gpuIndex);
                return;
            }
            catch 
            {
                Logger.LogWarning("Failed to create DML Execution Provider on Windows. Falling back to CUDA..."); 
            };

            // If the user's system does not support DirectML (for whatever reason,
            // it's shipped with Windows 10, version 1903(10.0; Build 18362)+
            // Fallback on good ol' CUDA
            try
            {
                sessionOptions.AppendExecutionProvider_CUDA(gpuIndex);
                return;
            }
            catch 
            {
                Logger.LogWarning("Failed to create CUDA Execution Provider on Windows.");
                Logger.LogWarning("No GPU acceleration will be applied.");

                Settings.UpdateSetting<bool>(nameof(Settings.GeneralSettings.GuiUseGpu), false.ToString());
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            // This will crash Linux users without Nvidia GPUs.
            // TODO: Fix this!
            try
            {
                sessionOptions.AppendExecutionProvider_CUDA(gpuIndex);
                return;
            }
            catch
            {
                Logger.LogWarning("Failed to create CUDA Execution Provider on Linux.");
                Logger.LogWarning("No GPU acceleration will be applied.");

                Settings.UpdateSetting<bool>(nameof(Settings.GeneralSettings.GuiUseGpu), false.ToString());
            }
        }
    }
}
