using Babble.Core;

namespace Babble.Maui;

public partial class MainPage : TabbedPage
{
    public MainPage()
    {
        InitializeComponent();
        // CameraAddress.Text = "help";
        // CameraImage.
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

    public async void OnInferenceThreadNumChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_inference_threads", args.NewTextValue);
    }

    public async void OnRuntimeChanged(object sender, EventArgs args)
    {

    }

    public async void OnGPUIndexChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_gpu_index", args.NewTextValue);
    }

    public async void OnMultiplierChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_multiply", args.NewTextValue);
    }

    public async void OnUseGPUToggled(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_use_gpu", args.NewTextValue);
    }

    public async void OnCalibrationDeadzoneChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("calib_deadzone", args.NewTextValue);
    }

    public async void OnMinCutoffChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_min_cutoff", args.NewTextValue);
    }

    public async void OnSpeedCoeffChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_speed_coefficient", args.NewTextValue);
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
