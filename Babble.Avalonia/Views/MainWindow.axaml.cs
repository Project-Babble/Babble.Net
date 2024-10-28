using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Babble.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void SaveAndRestartTracking_Click(object sender, RoutedEventArgs e)
    {
        // Logic for saving and restarting tracking
    }

    private void TrackingMode_Click(object sender, RoutedEventArgs e)
    {
        // Logic for entering tracking mode
    }

    private void CroppingMode_Click(object sender, RoutedEventArgs e)
    {
        // Logic for entering cropping mode
    }

    private void RotationSlider_ValueChanged(object sender, RoutedEventArgs e)
    {
        // Logic for handling rotation slider changes
    }

    private void StartCalibration_Click(object sender, RoutedEventArgs e)
    {
        // Logic for starting calibration
    }

    private void StopCalibration_Click(object sender, RoutedEventArgs e)
    {
        // Logic for stopping calibration
    }

    private void EnableCalibration_Checked(object sender, RoutedEventArgs e)
    {
        // Logic for enabling calibration
    }

    private void EnableCalibration_Unchecked(object sender, RoutedEventArgs e)
    {
        // Logic for disabling calibration
    }

    private void VerticalFlip_Checked(object sender, RoutedEventArgs e)
    {
        // Logic for vertical flip
    }

    private void HorizontalFlip_Checked(object sender, RoutedEventArgs e)
    {
        // Logic for horizontal flip
    }
}
