﻿using Babble.Core.Enums;
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
public class BabbleCore
{
    public static BabbleCore Instance { get; private set; }

    public BabbleSettings Settings { get; private set; }

    internal ILogger Logger { get; private set; }
    
    private PlatformConnector _platformConnector;
    private InferenceSession _session;
    private string _inputName;
    private bool _isRunning;

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
            if (setting == "capture_source")
            {
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
        if (_isRunning)
        {
            Logger.LogInformation("Babble.Core was already running. Restarting...");
            Stop();
        }

        Settings = settings;

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger(nameof(BabbleCore));

        // TODO Support a multi-model system
        // For the time being one model is enough
        // Model manifest on Github?
        const string modelName = "model.onnx";
        string modelPath = Path.Combine(AppContext.BaseDirectory, modelName);

        // Extract the embedded model if it isn't already present
        if (!File.Exists(modelPath))
        {
            using var stm = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream($"Babble.Core.{modelName}");

            using Stream outFile = File.Create(modelPath);

            const int sz = 4096;
            var buf = new byte[sz];
            while (true)
            {
                if (stm == null) continue;
                var nRead = stm.Read(buf, 0, sz);
                if (nRead < 1)
                    break;
                outFile.Write(buf, 0, nRead);
            }
        }

        SessionOptions sessionOptions = SetupSessionOptions();

        ConfigurePlatformSpecificGPU(sessionOptions);
        ConfigurePlatformConnector();

        _session = new InferenceSession(modelPath, sessionOptions);
        _inputName = _session.InputMetadata.Keys.First().ToString();
        _isRunning = true;

        Logger.LogInformation("Babble.Core started.");
    }

    /// <summary>
    /// Poll expression data
    /// </summary>
    /// <param name="UnifiedExpressions"></param>
    /// <returns></returns>
    public bool GetExpressionData(out Dictionary<UnifiedExpression, float> UnifiedExpressions)
    {
        // Cache/Clear dictionary on start?
        UnifiedExpressions = new();

        if (!_isRunning || Instance is null)
        {
            Logger.LogError("Tried to to poll Babble.Core, but it wasn't running!");
            return false;
        }

        var arKitExpressions = Utils.ARKitExpressions;

        // Camera not ready, or connecting to new source
        var data = _platformConnector.ExtractFrameData();
        if (data is null)
        {
            return false;
        }
        if (data.Length == 0)
        {
            return false;
        }

        var inputTensor = Utils.PreprocessFrame(data);

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, inputTensor)
        };

        using var results = _session.Run(inputs);

        var output = results[0].AsEnumerable<float>().ToArray();

        for (int i = 0; i < output.Length; i++)
        {
            arKitExpressions[(ARKitExpression)i] = Math.Clamp(output[i], 0f, 1f);
        }

        // Map Babble's ARKit to UnifiedExpression
        foreach (var exp in Utils.ExpressionMapping)
        {
            foreach (var ue in exp.Value)
            {
                UnifiedExpressions.Add(ue, arKitExpressions[exp.Key]);
            }
        }


        return true;
    }

    /// <summary>
    /// Gets the normalized pre-transform lip image for this frame
    /// This image will be (dimensions.width)px * (dimensions.height)px, Rgb888x
    /// </summary>
    /// <param name="image"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public bool GetImage(out byte[] image, out (int width, int height) dimensions)
    {
        dimensions = (0, 0);
        image = Array.Empty<byte>();
        if (_platformConnector.Capture.Frame is null)
        {
            return false;
        }

        dimensions = _platformConnector.Capture.Dimensions;
        if (_platformConnector.Capture.Frame.NumberOfChannels != 1)
        {
            using var grayMat = new Mat();
            CvInvoke.CvtColor(_platformConnector.Capture.Frame, grayMat, ColorConversion.Bgr2Gray);
            image = grayMat.GetRawData();
        }
        else
        {
            image = _platformConnector.Capture.Frame.GetRawData();
        }
        
        return true;
    }

    /// <summary>
    /// Stop things
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
        {
            Logger.LogWarning("Tried to to stop Babble.Core, but it wasn't running!");
            return;
        }

        _isRunning = false;
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
        sessionOptions.IntraOpNumThreads = Settings.GetSetting<int>($"gui_inference_threads");
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
        if (Settings.GetSetting<bool>("gui_use_gpu"))
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
            else if (RuntimeDetector.IsRuntimeSupported(Runtime.CUDA))
            {
                // The EmguCV CUDA runtime BLOWS UP how much ram we use?!
                // Like 150mb to 1.2GB!!
                var gpuIndex = Settings.GetSetting<int>("gui_gpu_index");
                sessionOptions.AppendExecutionProvider_CUDA(gpuIndex);
            }
        }
        else
        {
            sessionOptions.AppendExecutionProvider_CPU();
        }
    }
}
