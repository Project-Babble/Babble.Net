using Babble.Core.Settings;
using Babble.Core.Settings.Models;
using System.Text.Json;

namespace Babble.Tests.Settings;

public class SettingsTests : IDisposable
{
    private static string _testConfigPath = 
        Path.Combine(AppContext.BaseDirectory, "AppConfiguration.json");

    private static BabbleSettings CreateTestConfiguration()
    {
        var testSettings = new BabbleSettings
        {
            Version = 1,
            CamDisplayId = "0",
            Cam = new CameraSettings
            {
                // Add appropriate test values for CameraSettings
            },
            GeneralSettings = new GeneralSettings
            {
                // Add appropriate test values for GeneralSettings
            }
        };

        var json = JsonSerializer.Serialize(testSettings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_testConfigPath, json);

        return testSettings;
    }

    [Fact]
    public void GetSetting_ReturnsCorrectValue_ForTopLevelProperty()
    {
        var _settings = CreateTestConfiguration();
        _settings.Load();

        // Arrange
        const int expectedVersion = 1;

        // Act
        var version = _settings.GetSetting<int>("Version");

        // Assert
        Assert.Equal(expectedVersion, version);
    }

    [Fact]
    public void GetSetting_ThrowsArgumentException_ForNonExistentProperty()
    {
        var _settings = CreateTestConfiguration();
        _settings.Load();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _settings.GetSetting<BabbleSettings>("NonExistentProperty"));
    }

    [Fact]
    public void UpdateSetting_ModifiesValue_ForTopLevelProperty()
    {
        var _settings = CreateTestConfiguration();
        _settings.Load();

        // Arrange
        const int newVersion = 2;

        // Act
        _settings.UpdateSetting<int>("Version", newVersion.ToString());
        var updatedVersion = _settings.GetSetting<int>("Version");

        // Assert
        Assert.Equal(newVersion, updatedVersion);
    }

    [Fact]
    public void UpdateSetting_SavesToFile_AfterModification()
    {
        var _settings = CreateTestConfiguration();
        _settings.Load();

        // Arrange
        const int newVersion = 3;

        // Act
        _settings.UpdateSetting<int>("Version", "3");

        // Assert
        var fileContent = File.ReadAllText(_testConfigPath);
        var deserializedSettings = JsonSerializer.Deserialize<BabbleSettings>(fileContent);
        Assert.Equal(newVersion, deserializedSettings.Version);
    }

    [Fact]
    public void GetSetting_ReturnsCorrectValue_ForNestedProperty()
    {
        var _settings = CreateTestConfiguration();
        _settings.Load();

        // Arrange
        const int testValue = 101;
        _settings.UpdateSetting<int>("Cam.roi_window_x", "101");

        // Act
        var value = _settings.GetSetting<int>("Cam.roi_window_x");

        // Assert
        Assert.Equal(testValue, value);
    }

    [Fact]
    public void UpdateSetting_ModifiesValue_ForNestedProperty()
    {
        var _settings = CreateTestConfiguration();
        _settings.Load();

        // Arrange
        const int newValue = 102;

        // Act
        _settings.UpdateSetting<int>("Cam.roi_window_x", newValue.ToString());
        var updatedValue = _settings.GetSetting<int>("Cam.roi_window_x");

        // Assert
        Assert.Equal(newValue, updatedValue);
    }

    [Fact]
    public void LoadFromFile_LoadsCorrectValues_FromJsonFile()
    {
        // Arrange
        const int expectedVersion = 0;
        const int expectedDisplayId = 0;

        // Act
        var settings = new BabbleSettings(); // This will load from file

        // Assert
        Assert.Equal(expectedVersion, settings.Version);
        Assert.Equal(expectedDisplayId.ToString(), settings.CamDisplayId);
        Assert.NotNull(settings.Cam);
        Assert.NotNull(settings.GeneralSettings);
    }

    public void Dispose()
    {
        // Clean up test configuration file
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }
}