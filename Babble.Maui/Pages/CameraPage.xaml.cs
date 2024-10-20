namespace Babble.Maui;

public partial class CameraPage : ContentPage
{
    public CameraPage()
    {
        InitializeComponent();
    }
    public async void SaveAndRestartTracking(object sender, EventArgs args)
    {

    }
    public async void TrackingMode(object sender, EventArgs args)
    {

    }

    public async void CroppingMode(object sender, EventArgs args)
    {

    }

    public async void StartCalibration(object sender, EventArgs args)
    { 
    
    }

    public async void StopCalibration(object sender, EventArgs args)
    { 
    
    }

    public async void CameraSource(object sender, EventArgs args)
    {
        Console.WriteLine(((Entry)sender).Text); // Print the text upon completion 
    }

    public async void SliderRotation(object sender, EventArgs args)
    {

    }

    public async void EnableCalibration(object sender, EventArgs args)
    { 
    
    }

    public async void VerticalFlip(object sender, EventArgs args)
    { 
    
    }

    public async void HorizontalFlip(object sender, EventArgs args)
    { 
    
    }
}