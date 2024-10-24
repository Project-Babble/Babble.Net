using Babble.Core;
using Babble.Maui.Pages;

namespace Babble.Maui;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnCheckForUpdatesToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<bool>($"{Prefixes.Settings}gui_update_check", args.Value.ToString());
    }

    private async void OnAddressChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_osc_address", args.NewTextValue);
    }

    private async void OnPortChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_osc_port", args.NewTextValue);
    }

    private async void OnReceiverPortChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_osc_receiver_port", args.NewTextValue);
    }

    private async void OnRecalibrateAddressChanged(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<bool>($"{Prefixes.Settings}gui_osc_recalibrate_address", args.Value.ToString());
    }

    private async void OnUseRedChannelToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<bool>($"{Prefixes.Settings}gui_use_red_channel", args.Value.ToString());
    }

    private async void OnXResChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_cam_resolution_x", args.NewTextValue);
    }

    private async void OnYResChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_cam_resolution_y", args.NewTextValue);
    }

    private async void OnFramerateChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_cam_framerate", args.NewTextValue);
    }
}
