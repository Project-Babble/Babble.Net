using Babble.Core.Enums;
using Babble.Core.Scripts;
using Babble.Core.Scripts.Decoders;
using Babble.Core.Scripts.EmguCV;
using Babble.Core.Settings;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using System.Reflection;

namespace Babble.Core;

/// <summary>
/// The singleton entrypoint for our library
/// </summary>
public partial class BabbleCore
{
    public static BabbleCore Instance { get; private set; }
    public BabbleSettings Settings { get; private set; }
    internal ILogger Logger { get; private set; }
    public bool IsRunning { get; private set; }
    
    private PlatformConnector _platformConnector;
    private InferenceSession _session;
    private string _inputName;

    private OneEuroFilter floatFilter;

    static BabbleCore()
    {
        Instance = new BabbleCore();
    }

    private BabbleCore()
    {       
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());

        Logger = factory.CreateLogger(nameof(BabbleCore));
        Settings = new BabbleSettings();
        Settings.OnUpdate += (setting) =>
        {
            // Hacky but it works
            var normalizedSetting = setting.Replace("_", string.Empty).ToLower();
            if (normalizedSetting == "capturesource")
            {
                if (_platformConnector is not null)
                    _platformConnector!.Terminate();
                ConfigurePlatformConnector();
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
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger(nameof(BabbleCore));

        // Model manifest on Github?
        const string defaultModelName = "model.onnx";
        string modelPath = Path.Combine(AppContext.BaseDirectory, defaultModelName);
        Utils.ExtractEmbeddedResource(
            Assembly.GetExecutingAssembly(), 
            Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.Contains(defaultModelName)).First(), // Babble model
            modelPath, 
            overwrite: false);

        if (File.Exists(Instance.Settings.GeneralSettings.GuiModelFile))
        {
            modelPath = Instance.Settings.GeneralSettings.GuiModelFile;
        }

        SessionOptions sessionOptions = SetupSessionOptions();
        ConfigurePlatformSpecificGPU(sessionOptions);

        var fps = settings.GeneralSettings.GuiCamFramerate > 0 ? settings.GeneralSettings.GuiCamFramerate : 30;
        var mc = settings.GeneralSettings.GuiMinCutoff > 0 ? settings.GeneralSettings.GuiMinCutoff : 1;
        var sc = settings.GeneralSettings.GuiSpeedCoefficient > 0 ? settings.GeneralSettings.GuiSpeedCoefficient : 0;
        ConfigurePlatformConnector();
        floatFilter = new OneEuroFilter(fps, mc);

        _session = new InferenceSession(modelPath, sessionOptions);
        _inputName = _session.InputMetadata.Keys.First().ToString();
        IsRunning = true;


        Logger.LogInformation("Babble.Core started.");
    }

    /// <summary>
    /// Poll expression data, frames
    /// </summary>
    /// <param name="UnifiedExpressions"></param>
    /// <returns></returns>
    public bool GetExpressionData(out Dictionary<UnifiedExpression, float> UnifiedExpressions)
    {
        UnifiedExpressions = null;
        if (!IsRunning || Instance is null)
        {
            Logger.LogError("Tried to to poll Babble.Core, but it wasn't running!");
            return false;
        }
        
        // Test if the camera is not ready or connecting to new source
        var data = _platformConnector.ExtractFrameData();
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
            arKitExpressions[(ARKitExpression)i] = Math.Clamp(output[i], 0f, 1f);
        }

        // Map unfiltered ARKit expressions to filtered Unified Expressions
        foreach (var exp in Utils.ExpressionMapping)
        {
            float filteredValue = arKitExpressions[exp.Key];
            foreach (var ue in exp.Value)
            {
                filteredValue = floatFilter.Filter(filteredValue);
                CachedExpressionTable[ue] = filteredValue;
            }
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
        if (_platformConnector.Capture.RawFrame is null)
        {
            return false;
        }

        dimensions = _platformConnector.Capture.Dimensions;
        if (_platformConnector.Capture.RawFrame.NumberOfChannels == 3)
        {
            using var grayMat = new Mat();
            CvInvoke.CvtColor(_platformConnector.Capture.RawFrame, grayMat, ColorConversion.Bgr2Gray);
            image = grayMat.GetRawData();
            return true;
        }
        else if (_platformConnector.Capture.RawFrame.NumberOfChannels == 1)
        {
            image = _platformConnector.Capture.RawFrame.GetRawData();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the prost-transform lip image for this frame
    /// This image will be 256*256px, Rgb888x
    /// </summary>
    /// <param name="image"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public bool GetImage(out byte[] image, out (int width, int height) dimensions)
    {
        dimensions = (0, 0);
        image = Array.Empty<byte>();
        using var transformedImageCandidate = _platformConnector.TransformRawImage();
        if (transformedImageCandidate is null) return false;

        dimensions = (256, 256);
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
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            _platformConnector = new MobileConnector(Settings.Cam.CaptureSource);
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
            if (OperatingSystem.IsAndroid())
            {
                // This will be deprecated in Android 15+
                // EmguCV TF might be a good option here
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
