using Babble.Core;
using Babble.Maui.Pages;

namespace Babble.Maui;

public partial class AlgorithmSettingsPage : ContentPage
{
    public AlgorithmSettingsPage()
	{
		InitializeComponent();
	}

    public async void OnInferenceThreadNumChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}settings.gui_inference_threads", args.NewTextValue);
    }

    public async void OnRuntimeChanged(object sender, EventArgs args)
    {
        
    }

    public async void OnGPUIndexChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_gpu_index", args.NewTextValue);
    }

    public async void OnMultiplierChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_multiply", args.NewTextValue);
    }

    public async void OnUseGPUToggled(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_use_gpu", args.NewTextValue);
    }

    public async void OnCalibrationDeadzoneChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}calib_deadzone", args.NewTextValue);
    }

    public async void OnMinCutoffChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_min_cutoff", args.NewTextValue);
    }

    public async void OnSpeedCoeffChanged(object sender, TextChangedEventArgs args)
    {
        BabbleCore.Settings.UpdateSetting<int>($"{Prefixes.Settings}gui_speed_coefficient", args.NewTextValue);
    }
}