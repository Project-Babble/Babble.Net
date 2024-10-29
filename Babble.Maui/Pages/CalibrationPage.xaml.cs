using Babble.Core;

namespace Babble.Maui;

public partial class CalibrationPage : ContentPage
{
	public CalibrationPage()
	{
		InitializeComponent();
	}

    public void OnCalibrationModeChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("calibration_mode", ((Entry)sender).Text);
    }
}