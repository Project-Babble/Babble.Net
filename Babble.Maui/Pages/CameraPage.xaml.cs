using Babble.Core;
using Babble.Maui.Pages;

namespace Babble.Maui;

public partial class CameraPage : ContentPage
{
    public CameraPage()
    {
        InitializeComponent();
    }

    public async void OnCameraAddressChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Camera}capture_source", args.NewTextValue);
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
        BabbleCore.Settings.UpdateSetting<bool>($"{Prefixes.Camera}use_calibration", args.Value.ToString());
    }

    public async void OnVerticalFlipToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<bool>($"{Prefixes.Camera}gui_vertical_flip", args.Value.ToString());
    }

    public async void OnHorizontalFlipToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<bool>($"{Prefixes.Camera}gui_horizontal_flip", args.Value.ToString());
    }
}