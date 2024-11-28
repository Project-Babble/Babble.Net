using Babble.Core;
using Babble.Core.Settings;
using System.Text.Json;

namespace Babble.Tests.Settings;

public class SettingsTests : IDisposable
{
    private static string _testConfigPath = 
        Path.Combine(AppContext.BaseDirectory, "AppConfiguration.json");

    private static BabbleSettings CreateTestConfiguration()
    {
        var testSettings = new BabbleSettings();

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
        var version = _settings.Version;

        // Assert
        Assert.Equal(expectedVersion, version);
    }

    [Fact]
    public void UpdateSetting_ModifiesValue_ForTopLevelProperty()
    {
        var _settings = CreateTestConfiguration();
        _settings.Load();

        // Arrange
        const int newVersion = 2;

        // Act
        _settings.UpdateSetting<int>(nameof(BabbleCore.Instance.Settings.Version), newVersion.ToString());
        var updatedVersion = _settings.Version;

        // Assert
        Assert.Equal(newVersion, updatedVersion);
    }

    [Fact]
    public void GetSetting_ReturnsCorrectValue_ForNestedProperty()
    {
        var _settings = CreateTestConfiguration();
        _settings.Load();

        // Arrange
        const int testValue = 101;
        _settings.UpdateSetting<int>(nameof(BabbleCore.Instance.Settings.Cam.RoiWindowX), "101");

        // Act
        var value = _settings.Cam.RoiWindowX;

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
        _settings.UpdateSetting<int>(nameof(BabbleCore.Instance.Settings.Cam.RoiWindowX), newValue.ToString());
        var updatedValue = _settings.Cam.RoiWindowX;

        // Assert
        Assert.Equal(newValue, updatedValue);
    }

    [Fact]
    public void LoadFromFile_LoadsCorrectValues_FromJsonFile()
    {
        // Arrange
        const int expectedVersion = 1;
        const int expectedDisplayId = 0;

        // Act
        var settings = new BabbleSettings(); // This will load from file

        // Assert
        Assert.Equal(expectedVersion, settings.Version);
        Assert.Equal(expectedDisplayId, settings.CamDisplayId);
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