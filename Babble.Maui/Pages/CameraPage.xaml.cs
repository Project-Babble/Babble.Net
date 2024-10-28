using Babble.Core;
using Babble.Core.Scripts;
using Microsoft.Maui.Graphics.Platform;

namespace Babble.Maui;

public partial class CameraPage : ContentPage
{
    public CameraPage()
    {
        InitializeComponent();
        CameraAddress.Text = BabbleCore.Instance.Settings.GetSetting<string>("capture_source");
    }

    public async void OnCameraAddressChanged(object sender, TextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(args.OldTextValue)) return;
        if (args.OldTextValue != args.NewTextValue)
            BabbleCore.Instance.Settings.UpdateSetting<int>("capture_source", args.NewTextValue);
    }

    public async void OnSaveAndRestartTrackingClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnTrackingModeClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnCroppingModeClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnSliderRotationChanged(object sender, EventArgs args)
    {
        
    }

    public async void OnStartCalibrationClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnStopCalibrationClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnEnableCalibrationToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("use_calibration", args.Value.ToString());
    }

    public async void OnVerticalFlipToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_vertical_flip", args.Value.ToString());
    }

    public async void OnHorizontalFlipToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_horizontal_flip", args.Value.ToString());
    }
}