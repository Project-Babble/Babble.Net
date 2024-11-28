using Babble.Core.Settings.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace Babble.Core.Settings;

public class BabbleSettings
{
    public event Action<string>? OnUpdate;

    [JsonProperty("version")]
    public int Version { get; private set; }

    [JsonProperty("cam")]
    public CameraSettings Cam { get; private set; }

    [JsonProperty("settings")]
    public GeneralSettings GeneralSettings { get; private set; }

    [JsonProperty("cam_display_id")]
    public int CamDisplayId { get; private set; }

    private static string AppConfigFile => Path.Combine(AppContext.BaseDirectory, "AppConfiguration.json");

    // Dictionary to cache PropertyInfo for faster access. Needs prefix dict below to work
    private readonly Dictionary<string, PropertyInfo> _propertyCache;
    private static readonly string[] prefixes = ["", "cam.", "generalsettings."];

    public BabbleSettings()
    {
        _propertyCache = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        CacheProperties(GetType());
        Version = 1;
        CamDisplayId = 0;
        Cam = new CameraSettings();
        GeneralSettings = new GeneralSettings();
    }

    // Caching properties for this and nested classes
    private void CacheProperties(Type type, string parent = "")
    {
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertyName = string.IsNullOrEmpty(parent) ? property.Name : $"{parent}.{property.Name}";

            if (!_propertyCache.ContainsKey(propertyName))
            {
                _propertyCache[propertyName] = property;

                // If the property type is a class and not a primitive type, cache its properties as well (recursively)
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    CacheProperties(property.PropertyType, propertyName);
                }
            }
        }
    }

    // Method to set property value by name
    public void UpdateSetting<T>(string propertyName, string value)
    {
        var propertyNameCopy = propertyName.Replace("_", string.Empty);
        foreach (var p in prefixes)
        {
            var fullPropertyName = p + propertyNameCopy;
            if (_propertyCache.TryGetValue(fullPropertyName, out var propertyInfo))
            {
                // Get the containing object for the property
                var target = GetPropertyTarget(fullPropertyName);

                // Convert the value to the correct type and set it
                var convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                propertyInfo.SetValue(target, convertedValue);

                Save();
                OnUpdate?.Invoke(propertyName);
                return;

                // If the user changes a setting that requires them to restart,
                // DON't force them in and out of the app! Be consistent with the 
                // current Babble App
                //if (BabbleCore.Instance.IsRunning)
                //{
                //    BabbleCore.Instance.Stop();
                //    BabbleCore.Instance.Start(this);
                //}
            }
        }

        throw new ArgumentException($"Property '{propertyName}' does not exist.");
    }

    private object? GetPropertyTarget(string propertyName)
    {
        var parts = propertyName.Split('.');
        if (parts.Length == 1)
        {
            return this;
        }

        // For nested properties, navigate to the parent object
        var parentPath = string.Join('.', parts.Take(parts.Length - 1));
        if (_propertyCache.TryGetValue(parentPath, out var parentProperty))
        {
            return parentProperty.GetValue(GetPropertyTarget(parentPath));
        }

        throw new ArgumentException($"Parent property '{parentPath}' does not exist.");
    }

    /// <summary>
    /// Method to save the settings to a JSON file
    /// </summary>
    public void Save()
    {       
        var json = JsonConvert.SerializeObject(this);
        File.WriteAllText(AppConfigFile, json);
    }

    /// <summary>
    /// Method to load the settings from a JSON file
    /// </summary>
    public void Load()
    {
        BabbleSettings? config = null;
        if (File.Exists(AppConfigFile))
        {
            var json = File.ReadAllText(AppConfigFile);
            config = JsonConvert.DeserializeObject<BabbleSettings>(json)!;
        }
        config ??= new BabbleSettings();

        Version = config.Version;
        Cam = config.Cam;
        GeneralSettings = config.GeneralSettings;
        CamDisplayId = config.CamDisplayId;
    }
}
