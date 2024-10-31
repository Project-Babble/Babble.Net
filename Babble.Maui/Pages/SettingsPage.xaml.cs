using Babble.Core;
using Babble.Maui.Locale;

namespace Babble.Maui;

public partial class SettingsPage : ContentPage, ILocalizable
{
    public SettingsPage()
    {
        InitializeComponent();

        var settings = BabbleCore.Instance.Settings;
        CheckForUpdates.IsChecked = settings.GetSetting<bool>("gui_update_check");
        LocationPrefix.Text = settings.GetSetting<string>("gui_osc_location");
        IPAddress.Text = settings.GetSetting<string>("gui_osc_address");
        SendPort.Text = settings.GetSetting<int>("gui_osc_port").ToString();
        ReceiverPort.Text = settings.GetSetting<int>("gui_osc_receiver_port").ToString();
        RecalibrateAddress.Text = settings.GetSetting<string>("gui_osc_recalibrate_address");
        UseRedChannel.IsChecked = settings.GetSetting<bool>("gui_use_red_channel");
        XRes.Text = settings.GetSetting<int>("gui_cam_resolution_x").ToString();
        YRes.Text = settings.GetSetting<int>("gui_cam_resolution_y").ToString();
        Framerate.Text = settings.GetSetting<int>("gui_cam_framerate").ToString();

        foreach (var lang in LocaleManager.Instance.GetLanguages())
        {
            LanguagePicker.Items.Add(lang);
        }

        LanguagePicker.SelectedIndex = 0;

        Localize();
        LocaleManager.OnLocaleChanged += Localize;
    }

    public void Localize()
    {
        GeneralSettingsText.Text = LocaleManager.Instance["general.header"];
        LocaleManager.SetLocalizedText(CheckForUpdatesText, "general.checkForUpdates", "general.tooltip");
        LocaleManager.SetLocalizedText(GeneralSettingsText, "general.header");
        LocaleManager.SetLocalizedText(CheckForUpdatesText, "general.checkForUpdates", "general.tooltip");
        LocaleManager.SetLocalizedText(OSCSettingsText, "general.oscSettings");
        LocaleManager.SetLocalizedText(LocationPrefixText, "general.locationPrefix", "general.locationTooltip");
        LocaleManager.SetLocalizedText(IPAddressText, "general.address", "general.addressTooltip");
        LocaleManager.SetLocalizedText(ReceiveFunctionsText, "general.receiver", "general.receiverTooltip");
        LocaleManager.SetLocalizedText(ReceiverPortText, "general.receiverPort", "general.receiverPortTooltip");
        LocaleManager.SetLocalizedText(RecalibrateAddressText, "general.recalibrate", "general.recalibrateTooltip");
        LocaleManager.SetLocalizedText(RecalibrateAddressText, "general.recalibrate", "general.recalibrateTooltip");
        UVCCameraSettingsText.Text = LocaleManager.Instance["general.uvcCameraSettings"];
        LocaleManager.SetLocalizedText(UseRedChannelText, "general.useRedChannel", "general.useRedChannelTooltip");
        LocaleManager.SetLocalizedText(XResText, "general.xResolution", "general.xResolutionTooltip");
        LocaleManager.SetLocalizedText(YResText, "general.yResolution", "general.yResolutionTooltip");
        LocaleManager.SetLocalizedText(FramerateText, "general.framerate", "general.framerateTooltip");
        LocaleManager.SetLocalizedText(LanguagePickerText, "general.language", "general.languageTooltip");
    }

    public void OnCheckForUpdatesToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_update_check", args.Value.ToString());
    }

    public void OnLocationPrefixChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<string>("gui_osc_location", ((Entry)sender).Text);
    }

    public void OnAddressChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_osc_address", ((Entry)sender).Text);
    }

    public void OnPortChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_osc_port", ((Entry)sender).Text);
    }

    public void OnReceiverPortChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_osc_receiver_port", ((Entry)sender).Text);
    }

    public void OnRecalibrateAddressChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_osc_recalibrate_address", ((Entry)sender).Text);
    }

    public void OnUseRedChannelToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_use_red_channel", args.Value.ToString());
    }

    public void OnXResChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_cam_resolution_x", ((Entry)sender).Text);
    }

    public void OnYResChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_cam_resolution_y", ((Entry)sender).Text);
    }

    public void OnFramerateChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_cam_framerate", ((Entry)sender).Text);
    }

    public void OnLanguagePicked(object sender, EventArgs args)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;
        LocaleManager.Instance.ChangeLanguage(selectedIndex);
        BabbleCore.Instance.Settings.UpdateSetting<string>("gui_language", LocaleManager.Instance.GetLanguage());
    }
}
