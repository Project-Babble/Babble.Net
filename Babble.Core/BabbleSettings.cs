using Babble.Core.Enums;
using Newtonsoft.Json;
using System.Reflection;

namespace Babble.Core;

public class BabbleSettings
{
    public Dictionary<SettingKey, object> Settings { get; private set; }

    private static string BabbleConfigFile => Path.Combine(
        Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location)!, "BabbleConfig.json");

    private static readonly Dictionary<SettingKey, object> _defaultSettings = new()

    {
        { SettingKey.ModelFile, "model.onnx" },
        { SettingKey.InferenceThreads, 2 },
        { SettingKey.Runtime, "ONNX" },
        { SettingKey.GpuIndex, 0 },
        { SettingKey.UseGpu, true },
        { SettingKey.ModelOutputMultiplier, 1.0f },
        { SettingKey.CalibrationDeadzone, 0.1f },
        { SettingKey.MinFrequencyCutoff, 0.9f },
        { SettingKey.SpeedCoefficient, 0.9f },
        { SettingKey.CameraAddress, "COM6" },
        { SettingKey.Rotation, 0 },
        { SettingKey.VerticalFlip, false },
        { SettingKey.HorizontalFlip, false }
    };

    public BabbleSettings()
    {
        Settings = GetBabbleConfig();
    }

    public static Dictionary<SettingKey, object> GetBabbleConfig()
    {
        if (!File.Exists(BabbleConfigFile))
        {
            var text = JsonConvert.SerializeObject(_defaultSettings);
            File.WriteAllText(BabbleConfigFile, text);
            return _defaultSettings;
        }
        else
        {
            string value = File.ReadAllText(BabbleConfigFile);
            return JsonConvert.DeserializeObject<Dictionary<SettingKey, object>>(value)!;
        }
    }

    public void Save()
    {
        var text = JsonConvert.SerializeObject(Settings);
        File.WriteAllText(BabbleConfigFile, text);
    }
}
