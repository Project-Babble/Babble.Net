using Babble.Core.Enums;
using Babble.Core.Scripts;
using Babble.Core.Scripts.Config;
using Babble.Core.Scripts.Decoders;
using Babble.Core.Scripts.EmguCV;
using Babble.Core.Settings;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Newtonsoft.Json;
using NReco.Logging.File;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace Babble.Core;

/// <summary>
/// The singleton entrypoint for our library
/// </summary>
public partial class BabbleCore
{
    public static BabbleCore Instance { get; private set; }
    public BabbleSettings Settings { get; private set; }
    public ILogger<BabbleCore> Logger { get; private set; }
    [MemberNotNullWhen(true, nameof(_platformConnector), nameof(_session), nameof(_floatFilter), nameof(_calibrationItems))]
    public bool IsRunning { get; private set; }
    
    private Dictionary<string, CalibrationItem>? _calibrationItems;
    private PlatformConnector? _platformConnector;
    private InferenceSession? _session;
    private OneEuroFilter? _floatFilter;
    private Stopwatch sw = Stopwatch.StartNew();
    private Size _inputSize;
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
                if (_platformConnector is not null)
                    _platformConnector!.Terminate();
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
            Assembly.GetExecutingAssembly().
                GetManifestResourceNames().
                Where(x => x.Contains(defaultModelName)).
                First(), // Babble model
            modelPath, 
            overwrite: false);

        if (File.Exists(Instance.Settings.GeneralSettings.GuiModelFile))
        {
            modelPath = Instance.Settings.GeneralSettings.GuiModelFile;
        }

        SessionOptions sessionOptions = SetupSessionOptions();
        ConfigurePlatformSpecificGPU(sessionOptions);

        ConfigurePlatformConnector();

        var fps = settings.GeneralSettings.GuiCamFramerate > 0 ? settings.GeneralSettings.GuiCamFramerate : 30;
        var minCutoff = settings.GeneralSettings.GuiMinCutoff > 0 ? settings.GeneralSettings.GuiMinCutoff : 1.0f;
        var speedCoeff = settings.GeneralSettings.GuiSpeedCoefficient > 0 ? settings.GeneralSettings.GuiSpeedCoefficient : 0.007f;
        _floatFilter = new OneEuroFilter(
            minCutoff: 1.0f,
            beta: 0.007f
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
        var data = _platformConnector.ExtractFrameData(_inputSize);
        if (data is null) return false;
        if (data.Length == 0) return false;

        // Camera ready, prepare Mat as DenseTensor
        var inputTensor = TensorUtils.PreprocessFrame(data);
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, inputTensor)
        };

        // Run inference!
        using var results = _session.Run(inputs);
        var output = results[0].AsEnumerable<float>().ToArray();

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
                filteredValue = _floatFilter.Filter(filteredValue, (float)sw.Elapsed.TotalSeconds - _lastTime);
                _lastTime = (float)sw.Elapsed.TotalSeconds;
                CachedExpressionTable[ue] = Math.Clamp(filteredValue, _calibrationItems[expressionName].Min, _calibrationItems[expressionName].Max);
            }
            j++;
        }

        UnifiedExpressions = CachedExpressionTable;
        return true;
    }

    /// <summary>
    /// Gets the pre-transform lip image for this frame
    /// This image will be (dimensions.width)px * (dimensions.height)px, Rgb888x
    /// </summary>
    /// <param name="image"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public bool GetRawImage(out byte[] image, out (int width, int height) dimensions)
    {
        dimensions = (0, 0);
        image = Array.Empty<byte>();
        if (_platformConnector?.Capture.RawMat is null)
        {
            return false;
        }

        dimensions = _platformConnector.Capture.Dimensions;
        if (_platformConnector.Capture.RawMat.NumberOfChannels == 3)
        {
            using var grayMat = new Mat();
            CvInvoke.CvtColor(_platformConnector.Capture.RawMat, grayMat, ColorConversion.Bgr2Gray);
            image = grayMat.GetRawData();
            return true;
        }
        if (_platformConnector.Capture.RawMat.NumberOfChannels == 1)
        {
            image = _platformConnector.Capture.RawMat.GetRawData();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the prost-transform lip image for this frame
    /// This image will be 256*256px, single-channel
    /// </summary>
    /// <param name="image"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public bool GetImage(out byte[] image, out (int width, int height) dimensions)
    {
        dimensions = (0, 0);
        image = Array.Empty<byte>();
        using var transformedImageCandidate = _platformConnector?.TransformRawImage(_inputSize);
        if (transformedImageCandidate is null) return false;

        dimensions = (transformedImageCandidate.Width, transformedImageCandidate.Height);
        image = transformedImageCandidate.GetRawData();
        if (image is null) return false;
        if (image.Length == 0) return false;
        
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
        _platformConnector.Terminate();
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
        // sessionOptions.ExecutionMode = ExecutionMode.ORT_PARALLEL; // Hmm
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
        if (OperatingSystem.IsAndroid())
        {
            _platformConnector = new AndroidConnector(Settings.Cam.CaptureSource);
        } 
        else if (OperatingSystem.IsIOS())
        {
            _platformConnector = new iOSConnector(Settings.Cam.CaptureSource);
        }
        else
        {
            // Else, for WinUI, macOS, watchOS, MacCatalyst, tvOS, Tizen, etc...
            // Use the standard EmguCV VideoCapture backend
            _platformConnector = new DesktopConnector(Settings.Cam.CaptureSource);
        }

        _platformConnector.Initialize();
    }

    /// <summary>
    /// Per-platform hardware accel. detection/activation
    /// </summary>
    /// <param name="sessionOptions"></param>
    private void ConfigurePlatformSpecificGPU(SessionOptions sessionOptions)
    {
        sessionOptions.AppendExecutionProvider_CPU();
        if (Settings.GeneralSettings.GuiUseGpu)
        {
            // "The Android Neural Networks API (NNAPI) is an Android C API designed for
            // running computationally intensive operations for machine learning on Android devices."
            // It was added in Android 8.1 and will be deprecated in Android 15
            if (OperatingSystem.IsAndroid() && 
                OperatingSystem.IsAndroidVersionAtLeast(8, 1) && // At least 8.1
                !OperatingSystem.IsAndroidVersionAtLeast(15))    // At most 15
            {
                // EmguCV TF might be a good option here instead
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
            // Replace Microsoft.ML.OnnxRuntime with Microsoft.ML.OnnxRuntime.Gpu for Desktop builds.
            // TODO: Add this to .csproj?
            else if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
            {
                var gpuIndex = Settings.GeneralSettings.GuiGpuIndex;
                sessionOptions.AppendExecutionProvider_CUDA(gpuIndex);
            }
        }
    }
}
