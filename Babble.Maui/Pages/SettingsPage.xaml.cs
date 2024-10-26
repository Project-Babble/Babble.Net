using Babble.Core;

namespace Babble.Maui;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnCheckForUpdatesToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_update_check", args.Value.ToString());
    }

    private async void OnAddressChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_osc_address", args.NewTextValue);
    }

    private async void OnPortChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_osc_port", args.NewTextValue);
    }

    private async void OnReceiverPortChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_osc_receiver_port", args.NewTextValue);
    }

    private async void OnRecalibrateAddressChanged(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_osc_recalibrate_address", args.Value.ToString());
    }

    private async void OnUseRedChannelToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_use_red_channel", args.Value.ToString());
    }

    private async void OnXResChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_cam_resolution_x", args.NewTextValue);
    }

    private async void OnYResChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_cam_resolution_y", args.NewTextValue);
    }

    private async void OnFramerateChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_cam_framerate", args.NewTextValue);
    }
}
