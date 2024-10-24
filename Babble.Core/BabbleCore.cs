using Babble.Core.Enums;
using Babble.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using System.Reflection;

namespace Babble.Core;

public static class BabbleCore
{
    public static BabbleSettings Settings = new();
    private static InferenceSession _session;
    private static bool _isInferencing;
    private static string _inputName;
    private static ILogger _logger;

    public static bool StartInference(string modelName = "model.onnx", string path = "")
    {
        if (_isInferencing)
            return false;

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = factory.CreateLogger(nameof(BabbleCore));

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
        _logger.LogInformation("Inference started");
        _inputName = _session.InputMetadata.Keys.First().ToString();

        _isInferencing = true;

        return true;
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
        _logger.LogInformation("Inference stopped");
        return true;     
    }
}
