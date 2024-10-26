using Babble.Core;

namespace Babble.Maui;

public partial class CameraPage : ContentPage
{
    public CameraPage()
    {
        InitializeComponent();
    }

    public async void OnCameraAddressChanged(object sender, TextChangedEventArgs args)
    {
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