using Babble.Core.Enums;
using Microsoft.ML.OnnxRuntime;
using System.Reflection;

namespace Babble.Core;

public static class BabbleCore
{
    private static BabbleLogger _logger;
    private static BabbleSettings _settings;
    private static InferenceSession _session;
    private static bool _isInferencing;
    private static string _inputName;

    private const int EXPECTED_SIZE = 256 * 256; // Grayscale 256x256px, 1 float per pixel, 0-1 range

    public static bool StartInference()
    {
        if (_isInferencing)
            return false;

        try
        {
            _settings = new BabbleSettings();
            _logger = new BabbleLogger("Logs");

            // Extract the embedded model
            string modelName = (string) _settings.Settings[SettingKey.ModelFile];
            if (!File.Exists(modelName))
            {
                using var stm = Assembly
                    .GetExecutingAssembly()
                    .GetManifestResourceStream($"Babble.Core.{modelName}");

                using Stream outFile = File.Create(modelName);

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

            var sessionOptions = new SessionOptions();

            // TODO: Add settings!
            //if ((bool)_settings.Settings[SettingKey.UseGpu])
            //{
            //    sessionOptions.AppendExecutionProvider_Tvm((string)_settings.Settings[SettingKey.GpuIndex]);
            //}
            //else
            //{
            //    sessionOptions.AppendExecutionProvider_CPU();
            //}

            // Set webcam resolution (optional)
            // _capture.FrameWidth = 640;
            // _capture.FrameHeight = 480;

            _session = new InferenceSession((string)_settings.Settings[SettingKey.ModelFile], sessionOptions);
            _logger.Log(LogLevel.Info, "Inference started");
            _inputName = _session.InputMetadata.Keys.First().ToString();

            _isInferencing = true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Failed to start inference: {ex.Message}");
            return false;
        }

        return true;
    }

    public static bool GetExpressionData(float[] frame, out Dictionary<UnifiedExpressions, float> Expressions)
    {
        Expressions = new Dictionary<UnifiedExpressions, float>
        {
            { UnifiedExpressions.CheekPuffLeft, 0f },
            { UnifiedExpressions.CheekPuffRight, 0f },
            { UnifiedExpressions.CheekSuckLeft, 0f },
            { UnifiedExpressions.CheekSuckRight, 0f },
            { UnifiedExpressions.JawOpen, 0f },
            { UnifiedExpressions.JawForward, 0f },
            { UnifiedExpressions.JawLeft, 0f },
            { UnifiedExpressions.JawRight, 0f },
            { UnifiedExpressions.NoseSneerLeft, 0f },
            { UnifiedExpressions.NoseSneerRight, 0f },
            { UnifiedExpressions.LipFunnelLowerLeft, 0f },
            { UnifiedExpressions.LipFunnelLowerRight, 0f },
            { UnifiedExpressions.LipFunnelUpperLeft, 0f },
            { UnifiedExpressions.LipFunnelUpperRight, 0f },
            { UnifiedExpressions.LipPuckerLowerLeft, 0f },
            { UnifiedExpressions.LipPuckerLowerRight, 0f },
            { UnifiedExpressions.LipPuckerUpperLeft, 0f },
            { UnifiedExpressions.LipPuckerUpperRight, 0f },
            { UnifiedExpressions.MouthUpperLeft, 0f },
            { UnifiedExpressions.MouthLowerLeft, 0f },
            { UnifiedExpressions.MouthUpperRight, 0f },
            { UnifiedExpressions.MouthLowerRight, 0f },
            { UnifiedExpressions.LipSuckUpperLeft, 0f },
            { UnifiedExpressions.LipSuckUpperRight, 0f },
            { UnifiedExpressions.LipSuckLowerLeft, 0f },
            { UnifiedExpressions.LipSuckLowerRight, 0f },
            { UnifiedExpressions.MouthRaiserUpper, 0f },
            { UnifiedExpressions.MouthRaiserLower, 0f },
            { UnifiedExpressions.MouthClosed, 0f },
            { UnifiedExpressions.MouthCornerPullLeft, 0f },
            { UnifiedExpressions.MouthCornerPullRight, 0f },
            { UnifiedExpressions.MouthFrownLeft, 0f },
            { UnifiedExpressions.MouthFrownRight, 0f },
            { UnifiedExpressions.MouthDimpleLeft, 0f },
            { UnifiedExpressions.MouthDimpleRight, 0f },
            { UnifiedExpressions.MouthUpperUpLeft, 0f },
            { UnifiedExpressions.MouthUpperUpRight, 0f },
            { UnifiedExpressions.MouthLowerDownLeft, 0f },
            { UnifiedExpressions.MouthLowerDownRight, 0f },
            { UnifiedExpressions.MouthPressLeft, 0f },
            { UnifiedExpressions.MouthPressRight, 0f },
            { UnifiedExpressions.MouthStretchLeft, 0f },
            { UnifiedExpressions.MouthStretchRight, 0f },
            { UnifiedExpressions.TongueOut, 0f },
            { UnifiedExpressions.TongueUp, 0f },
            { UnifiedExpressions.TongueDown, 0f },
            { UnifiedExpressions.TongueLeft, 0f },
            { UnifiedExpressions.TongueRight, 0f },
            { UnifiedExpressions.TongueRoll, 0f },
            { UnifiedExpressions.TongueBendDown, 0f },
            { UnifiedExpressions.TongueCurlUp, 0f },
            { UnifiedExpressions.TongueSquish, 0f },
            { UnifiedExpressions.TongueFlat, 0f },
            { UnifiedExpressions.TongueTwistLeft, 0f },
            { UnifiedExpressions.TongueTwistRight, 0f }
        };

        if (!_isInferencing)
            return false;

        if (frame.Length != EXPECTED_SIZE)
            return false;

        var inputTensor = Utils.PreprocessFrame(frame);
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(_inputName, inputTensor) };

        using var results = _session.Run(inputs);
        var output = results[0].AsEnumerable<float>().ToArray();

        for (int i = 0; i < output.Length; i++)
        {
            Expressions[(UnifiedExpressions)i] = Math.Clamp(output[i], 0f, 1f);
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

    //public static bool UpdateSetting(SettingKey key, object value)
    //{
    //    if (_settings.Settings.ContainsKey(key))
    //    {
    //        _settings.Settings[key] = value;
    //        _settings.Save();
    //        _logger.Log(LogLevel.Info, $"Updated setting: {key} to {value}");

    //        // If inference is running, restart it to apply new settings
    //        if (_isInferencing)
    //        {
    //            StopInference();
    //            StartInference();
    //        }

    //        return true;
    //    }
    //    else
    //    {
    //        _logger.Log(LogLevel.Warning, $"Warning: Unknown setting '{key}'");
    //        return false;
    //    }
    //}
}
