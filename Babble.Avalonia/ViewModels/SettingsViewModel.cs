using Avalonia.Controls;
using Babble.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public ComboBox languageCombo;

    [ObservableProperty]
    public bool checkForUpdates;

    [ObservableProperty]
    public string ipAddress;

    [ObservableProperty]
    public string locationPrefix;

    [ObservableProperty]
    public int port;

    [ObservableProperty]
    public bool receiveFunctions;

    [ObservableProperty]
    public int receiverPort;

    [ObservableProperty]
    public string recalibrateAddress;

    [ObservableProperty]
    public bool forceRelevancy;

    [ObservableProperty]
    public bool useRedChannel;

    [ObservableProperty]
    public int xResolution;

    [ObservableProperty]
    public int yResolution;

    [ObservableProperty]
    public int framerate;

    [ObservableProperty]
    public string selectedLanguage;

    [ObservableProperty]
    public int selectedIndex;

    public SettingsViewModel()
    {
        var settings = BabbleCore.Instance.Settings;
        CheckForUpdates = settings.GeneralSettings.GuiUpdateCheck;
        LocationPrefix = settings.GeneralSettings.GuiOscLocation;
        IpAddress = settings.GeneralSettings.GuiOscAddress;
        Port = settings.GeneralSettings.GuiOscPort;
        ReceiverPort = settings.GeneralSettings.GuiOscReceiverPort;
        RecalibrateAddress = settings.GeneralSettings.GuiOscRecalibrateAddress;
        UseRedChannel = settings.GeneralSettings.GuiUseRedChannel;
        ForceRelevancy = settings.GeneralSettings.GuiForceRelevancy;
        XResolution = settings.GeneralSettings.GuiCamResolutionX;
        YResolution = settings.GeneralSettings.GuiCamResolutionY;
        Framerate = settings.GeneralSettings.GuiCamFramerate;
    }
}
