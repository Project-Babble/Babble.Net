using Babble.Core;

namespace Babble.Maui;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        CheckForUpdates.IsChecked = BabbleCore.Instance.Settings.GetSetting<bool>("gui_update_check");
        IPAddress.Text = BabbleCore.Instance.Settings.GetSetting<string>("gui_osc_address");
        SendPort.Text = BabbleCore.Instance.Settings.GetSetting<int>("gui_osc_port").ToString();
        ReceiverPort.Text = BabbleCore.Instance.Settings.GetSetting<int>("gui_osc_receiver_port").ToString();
        RecalibrateAddress.Text = BabbleCore.Instance.Settings.GetSetting<string>("gui_osc_recalibrate_address");
        UseRedChannel.IsChecked = BabbleCore.Instance.Settings.GetSetting<bool>("gui_use_red_channel");
        XRes.Text = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_resolution_x").ToString();
        YRes.Text = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_resolution_y").ToString();
        Framerate.Text = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_framerate").ToString();
    }

    public void OnCheckForUpdatesToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_update_check", args.Value.ToString());
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
}
