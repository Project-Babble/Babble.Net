using Avalonia.Controls;
using Babble.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    public ComboBox? languageCombo;

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
    public bool useOSCQuery;

    [ObservableProperty]
    public bool useRedChannel;

    [ObservableProperty]
    public int xResolution;

    [ObservableProperty]
    public int yResolution;

    [ObservableProperty]
    public int framerate;

    [ObservableProperty]
    public string? selectedLanguage;

    [ObservableProperty]
    public int selectedIndex;

    public SettingsViewModel()
    {
        var settings = BabbleCore.Instance.Settings;
        checkForUpdates = settings.GeneralSettings.GuiUpdateCheck;
        locationPrefix = settings.GeneralSettings.GuiOscLocation;
        ipAddress = settings.GeneralSettings.GuiOscAddress;
        port = settings.GeneralSettings.GuiOscPort;
        receiverPort = settings.GeneralSettings.GuiOscReceiverPort;
        recalibrateAddress = settings.GeneralSettings.GuiOscRecalibrateAddress;
        useOSCQuery = settings.GeneralSettings.GuiForceRelevancy;
        useRedChannel = settings.GeneralSettings.GuiUseRedChannel;
        xResolution = settings.GeneralSettings.GuiCamResolutionX;
        yResolution = settings.GeneralSettings.GuiCamResolutionY;
        framerate = settings.GeneralSettings.GuiCamFramerate;
    }
}
