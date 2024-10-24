using Babble.Core.Enums;
using Babble.Core.Scripts;
using Babble.Core.Scripts.Decoders;
using Babble.Core.Settings;
using Babble.OSC;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Babble.Core;

/// <summary>
/// The singleton entrypoint for our library
/// The EmguCV CUDA runtime BLOWS UP how much ram we use?? Like 150mb to 1.2GB!!
/// </summary>
public static class BabbleCore
{
    public static PlatformConnector PlatformConnector { get; private set; }

    public static BabbleSettings Settings { get; private set; }

    public static ILogger Logger { get; private set; }

    private static InferenceSession _session;

    private static bool _isInferencing;

    private static string _inputName;

    private static readonly BabbleOSC _sender;

    private static readonly Thread _thread;

    static BabbleCore()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger(nameof(BabbleCore));

        // Debugging values
        const string _lang = "English";
        const string _ip = @"192.168.0.75";
        const string _ipCameraUrl = @$"http://{_ip}:4747/video";
        var randomCameraUrl = DeviceEnumerator.EnumerateCameras(out var cameraMap) ?
            cameraMap.ElementAt(Random.Shared.Next(cameraMap.Count)).Key.ToString() : "0";

        // LocaleManager.Initialize(_lang);

        PlatformConnector = SetupPlatform(randomCameraUrl);
        PlatformConnector.Initialize();

        // TODO Pass in user's Quest headset address here!
        _sender = new BabbleOSC(_ip);
        _thread = new Thread(new ThreadStart(Update));
        _thread.Start();
    }

    public static bool StartInference(string modelName = "model.onnx", string path = "")
    {
        if (_isInferencing)
            return false;

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger(nameof(BabbleCore));

        string modelPath;
        if (string.IsNullOrEmpty(path))
        {
            modelPath = Path.Combine(AppContext.BaseDirectory, modelName);
        }
        else
        {
            modelPath = path;
        }

        // Extract the embedded model
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

        // TODO: Add settings!
        var sessionOptions = new SessionOptions();
        
        // Set webcam resolution (optional)
        // _capture.FrameWidth = 640;
        // _capture.FrameHeight = 480;

        _session = new InferenceSession(modelPath, sessionOptions);
        Logger.LogInformation("Inference started");
        _inputName = _session.InputMetadata.Keys.First().ToString();

        _isInferencing = true;

        return true;
    }

    /// <summary>
    /// Creates the proper video streaming classes based on the platform we're deploying to.
    /// EmguCV doesn't have support for VideoCapture on Android, iOS, or UWP
    /// We have a custom implementations for IP Cameras, the de-facto use case on mobile
    /// As well as SerialCameras (not tested on mobile yet)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    private static PlatformConnector SetupPlatform(string url)
    {
        if (OperatingSystem.IsAndroid() ||
            OperatingSystem.IsIOS() )
        {

            return new MobileConnector(@"http://192.168.0.75:4747/video"); // url
        }
        else
        {
            // Else, for WinUI, macOS, watchOS, MacCatalyst, tvOS, Tizen, etc...
            // Use the standard EmguCV VideoCapture backend
            return new DesktopConnector(url);
        }
    }

    private static void Update()
    {
        while (true)
        {
            var data = PlatformConnector.GetFrameData();

            if (!GetExpressionData(data, out var expressions))
                goto End;

            foreach (var exp in expressions)
                BabbleOSC.Expressions.SetByKey1(exp.Key, exp.Value);

            End:
            Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
        }
    }

    public static bool GetExpressionData(float[] frame, out Dictionary<UnifiedExpression, float> UnifiedExpressions)
    {
        // Cache/Clear dictionary on start?
        UnifiedExpressions = new();

        var arKitExpressions = Utils.ARKitExpressions;

        if (!_isInferencing)
            return false;

        var inputTensor = Utils.PreprocessFrame(frame);
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

    public static bool StopInference()
    {
        if (!_isInferencing)
            return false;
        
        _isInferencing = false;
        _session.Dispose();
        Logger.LogInformation("Inference stopped");
        return true;     
    }
}
