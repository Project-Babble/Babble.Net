﻿using Babble.Core.Settings.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Babble.Core.Settings;

public sealed class BabbleSettings
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("cam")]
    public CameraSettings Cam { get; set; }

    [JsonPropertyName("settings")]
    public GeneralSettings GeneralSettings { get; set; }

    [JsonPropertyName("cam_display_id")]
    public string CamDisplayId { get; set; }

    private static string AppConfigFile => Path.Combine(AppContext.BaseDirectory, "AppConfiguration.json");

    // Dictionary to cache PropertyInfo for faster access
    private readonly Dictionary<string, PropertyInfo> _propertyCache;

    public BabbleSettings()
    {
        _propertyCache = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        CacheProperties(GetType());
        Version = 0;
        CamDisplayId = "0";
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

    // Method to get property value by name
    public T GetSetting<T>(string propertyName)
    {
        propertyName = propertyName.Replace("_", string.Empty);

        // This is hacky. This MUST return something
        var prefixes = new string[] { "", "cam.", "generalsettings." };
        foreach (var p in prefixes)
        {
            var fullPropertyName = p + propertyName;
            if (_propertyCache.TryGetValue(fullPropertyName, out var propertyInfo))
            {
                var target = GetPropertyTarget(fullPropertyName);
                return (T)propertyInfo.GetValue(target);
            }
        }
        throw new ArgumentException($"Property '{propertyName}' does not exist.");
    }

    // Method to set property value by name
    public void UpdateSetting<T>(string propertyName, string value)
    {
        propertyName = propertyName.Replace("_", string.Empty);
        if (_propertyCache.TryGetValue(propertyName, out var propertyInfo))
        {
            // Get the containing object for the property
            var target = GetPropertyTarget(propertyName);

            // Convert the value to the correct type and set it
            var convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
            propertyInfo.SetValue(target, convertedValue);

            Save();

            // If we were already running, and we NEED to restart, restart
            // TODO Add settings whitelist. What settings should restart the app?
            if (BabbleCore.Instance.IsRunning && false)
            {
                BabbleCore.Instance.Stop();
                BabbleCore.Instance.Start(this);
            }
        }
        else
        {
            throw new ArgumentException($"Property '{propertyName}' does not exist.");
        }
    }

    private object GetPropertyTarget(string propertyName)
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
    public void Save(bool restart = false)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(AppConfigFile, json);
    }

    /// <summary>
    /// Method to load the settings from a JSON file
    /// </summary>
    public void Load()
    {
        var json = File.ReadAllText(AppConfigFile);
        var config = JsonSerializer.Deserialize<BabbleSettings>(json);
        Version = config.Version;
        Cam = config.Cam;
        GeneralSettings = config.GeneralSettings;
        CamDisplayId = config.CamDisplayId;
    }
}