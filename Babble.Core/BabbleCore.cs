using Babble.Core.Enums;
using Microsoft.ML.OnnxRuntime;
using System.Reflection;

namespace Babble.Core;

public static class BabbleCore
{

#pragma warning disable CS8618
    private static BabbleLogger _logger;
    private static BabbleSettings _settings;
    private static InferenceSession _session;
    private static bool _isInferencing;
    private static string _inputName;
#pragma warning restore CS8618 

    public static bool StartInference()
    {
        if (_isInferencing)
            return false;

        _settings = new BabbleSettings();
        _logger = new BabbleLogger("Logs");

        // Extract the embedded model
        var modelName = (string) _settings.Settings[SettingKey.ModelFile];   
        var modelPath = Path.Combine(AppContext.BaseDirectory, modelName);
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
        _logger.Log(LogLevel.Info, "Inference started");
        _inputName = _session.InputMetadata.Keys.First().ToString();

        _isInferencing = true;

        return true;
    }

    public static bool GetExpressionData(float[] frame, out Dictionary<UnifiedExpression, float> UnifiedExpressions)
    {
        // Cache/Clear dictionary on start?
        UnifiedExpressions = new();

        var arKitExpressions = new Dictionary<ARKitExpression, float>
        {
            { ARKitExpression.CheekPuffLeft, 0f },
            { ARKitExpression.CheekPuffRight, 0f },
            { ARKitExpression.CheekSuckLeft, 0f },
            { ARKitExpression.CheekSuckRight, 0f },
            { ARKitExpression.JawOpen, 0f },
            { ARKitExpression.JawForward, 0f },
            { ARKitExpression.JawLeft, 0f },
            { ARKitExpression.JawRight, 0f },
            { ARKitExpression.NoseSneerLeft, 0f },
            { ARKitExpression.NoseSneerRight, 0f },
            { ARKitExpression.MouthFunnel, 0f },
            { ARKitExpression.MouthPucker, 0f },
            { ARKitExpression.MouthLeft, 0f },
            { ARKitExpression.MouthRight, 0f },
            { ARKitExpression.MouthRollUpper, 0f },
            { ARKitExpression.MouthRollLower, 0f },
            { ARKitExpression.MouthShrugUpper, 0f },
            { ARKitExpression.MouthShrugLower, 0f },
            { ARKitExpression.MouthClose, 0f },
            { ARKitExpression.MouthSmileLeft, 0f },
            { ARKitExpression.MouthSmileRight, 0f },
            { ARKitExpression.MouthFrownLeft, 0f },
            { ARKitExpression.MouthFrownRight, 0f },
            { ARKitExpression.MouthDimpleLeft, 0f },
            { ARKitExpression.MouthDimpleRight, 0f },
            { ARKitExpression.MouthUpperUpLeft, 0f },
            { ARKitExpression.MouthUpperUpRight, 0f },
            { ARKitExpression.MouthLowerDownLeft, 0f },
            { ARKitExpression.MouthLowerDownRight, 0f },
            { ARKitExpression.MouthPressLeft, 0f },
            { ARKitExpression.MouthPressRight, 0f },
            { ARKitExpression.MouthStretchLeft, 0f },
            { ARKitExpression.MouthStretchRight, 0f },
            { ARKitExpression.TongueOut, 0f },
            { ARKitExpression.TongueUp, 0f },
            { ARKitExpression.TongueDown, 0f },
            { ARKitExpression.TongueLeft, 0f },
            { ARKitExpression.TongueRight, 0f },
            { ARKitExpression.TongueRoll, 0f },
            { ARKitExpression.TongueBendDown, 0f },
            { ARKitExpression.TongueCurlUp, 0f },
            { ARKitExpression.TongueSquish, 0f },
            { ARKitExpression.TongueFlat, 0f },
            { ARKitExpression.TongueTwistLeft, 0f },
            { ARKitExpression.TongueTwistRight, 0f }
        };

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
        _logger.Log(LogLevel.Info, "Inference stopped");
        return true;     
    }

    public static bool UpdateSetting(SettingKey key, object value)
    {
        if (_settings.Settings.ContainsKey(key))
        {
            _settings.Settings[key] = value;
            _settings.Save();
            _logger.Log(LogLevel.Info, $"Updated setting: {key} to {value}");

            // If inference is running, restart it to apply new settings
            if (_isInferencing)
            {
                StopInference();
                StartInference();
            }

            return true;
        }
        else
        {
            _logger.Log(LogLevel.Warning, $"Warning: Unknown setting '{key}'");
            return false;
        }
    }
}
