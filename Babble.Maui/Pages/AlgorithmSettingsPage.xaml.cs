using Babble.Core;

namespace Babble.Maui;

public partial class AlgorithmSettingsPage : ContentPage
{
    public AlgorithmSettingsPage()
	{
		InitializeComponent();
        InferenceThreads.Text = BabbleCore.Instance.Settings.GetSetting<int>("gui_inference_threads").ToString();
        UseGPU.IsChecked = BabbleCore.Instance.Settings.GetSetting<bool>("gui_use_gpu");
        GPUIndex.Text = BabbleCore.Instance.Settings.GetSetting<int>("gui_gpu_index").ToString();
        ModelMultiplier.Text = BabbleCore.Instance.Settings.GetSetting<double>("gui_multiply").ToString();

        // PickerItems.add()...
    }

    public void OnInferenceThreadNumChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_inference_threads", ((Entry)sender).Text);
    }

    public void OnRuntimeChanged(object sender, EventArgs args)
    {
        
    }

    public void OnGPUIndexChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_gpu_index", ((Entry)sender).Text);
    }

    public void OnMultiplierChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<double>("gui_multiply", ((Entry)sender).Text);
    }

    public void OnUseGPUToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_use_gpu", args.Value.ToString());
    }

    public void OnCalibrationDeadzoneChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("calib_deadzone", ((Entry)sender).Text);
    }

    public void OnMinCutoffChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_min_cutoff", ((Entry)sender).Text);
    }

    public void OnSpeedCoeffChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("gui_speed_coefficient", ((Entry)sender).Text);
    }
}