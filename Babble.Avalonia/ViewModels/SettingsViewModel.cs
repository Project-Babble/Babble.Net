using Avalonia.Controls;
using Babble.Core;
using Babble.Locale;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ViewModels;

public partial class SettingsViewModel : ObservableObject, ILocalizable
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

    public string GeneralSettingsText { get; private set; }
    public string GeneralSettingsTextTooltip { get; private set; }
    public string CheckForUpdatesText { get; private set; }
    public string CheckForUpdatesTextTooltip { get; private set; }
    public string OSCSettingsText { get; private set; }
    public string OSCSettingsTextTooltip { get; private set; }
    public string LocationPrefixText { get; private set; }
    public string LocationPrefixTextTooltip { get; private set; }
    public string IpAddressText { get; private set; }
    public string IpAddressTextTooltip { get; private set; }
    public string PortText { get; private set; }
    public string PortTextTooltip { get; private set; }
    public string ReceiveFunctionsText { get; private set; }
    public string ReceiveFunctionsTextTooltip { get; private set; }
    public string ReceiverPortText { get; private set; }
    public string ReceiverPortTextTooltip { get; private set; }
    public string RecalibrateAddressText { get; private set; }
    public string RecalibrateAddressTextTooltip { get; private set; }
    public string UVCCameraSettingsText { get; private set; }
    public string UVCCameraSettingsTextTooltip { get; private set; }
    public string UseRedChannelText { get; private set; }
    public string UseRedChannelTextTooltip { get; private set; }
    public string XResolutionText { get; private set; }
    public string XResolutionTextTooltip { get; private set; }
    public string YResolutionText { get; private set; }
    public string YResolutionTextTooltip { get; private set; }
    public string FramerateText { get; private set; }
    public string FramerateTextTooltip { get; private set; }
    public string LanguageText { get; private set; }
    public string LanguageInstructions { get; private set; }
    public string LanguageTextTooltip { get; private set; }


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

        Localize();
        LocaleManager.OnLocaleChanged += Localize;
    }

    public void Localize()
    {
        GeneralSettingsText = LocaleManager.Instance["general.header"];
        GeneralSettingsTextTooltip = LocaleManager.Instance["general.headerTooltip"];
        CheckForUpdatesText = LocaleManager.Instance["general.checkForUpdates"];
        CheckForUpdatesTextTooltip = LocaleManager.Instance["general.checkForUpdatesTooltip"];
        OSCSettingsText = LocaleManager.Instance["general.oscSettings"];
        OSCSettingsTextTooltip = LocaleManager.Instance["general.oscSettingsTooltip"];
        LocationPrefixText = LocaleManager.Instance["general.locationPrefix"];
        LocationPrefixTextTooltip = LocaleManager.Instance["general.locationPrefixTooltip"];
        IpAddressText = LocaleManager.Instance["general.address"];
        IpAddressTextTooltip = LocaleManager.Instance["general.addressTooltip"];
        PortText = LocaleManager.Instance["general.port"];
        PortTextTooltip = LocaleManager.Instance["general.portTooltip"];
        ReceiveFunctionsText = LocaleManager.Instance["general.receiver"];
        ReceiveFunctionsTextTooltip = LocaleManager.Instance["general.receiverTooltip"];
        ReceiverPortText = LocaleManager.Instance["general.receiverPort"];
        ReceiverPortTextTooltip = LocaleManager.Instance["general.receiverPortTooltip"];
        RecalibrateAddressText = LocaleManager.Instance["general.recalibrate"];
        RecalibrateAddressTextTooltip = LocaleManager.Instance["general.recalibrateTooltip"];
        UVCCameraSettingsText = LocaleManager.Instance["general.uvcCameraSettings"];
        UVCCameraSettingsTextTooltip = LocaleManager.Instance["general.uvcCameraSettingsTooltip"];
        UseRedChannelText = LocaleManager.Instance["general.useRedChannel"];
        UseRedChannelTextTooltip = LocaleManager.Instance["general.useRedChannelTooltip"];
        XResolutionText = LocaleManager.Instance["general.xResolution"];
        XResolutionTextTooltip = LocaleManager.Instance["general.xResolutionTooltip"];
        YResolutionText = LocaleManager.Instance["general.yResolution"];
        YResolutionTextTooltip = LocaleManager.Instance["general.yResolutionTooltip"];
        FramerateText = LocaleManager.Instance["general.framerate"];
        FramerateTextTooltip = LocaleManager.Instance["general.framerateTooltip"];
        LanguageText = LocaleManager.Instance["general.language"];
        LanguageInstructions = LocaleManager.Instance["general.languageInstructions"];
        LanguageTextTooltip = LocaleManager.Instance["general.languageTooltip"];
    }
}
