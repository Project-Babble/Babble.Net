using Avalonia.Media.Imaging;
using Babble.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class CamViewModel : ObservableObject
{
    [ObservableProperty]
    private string cameraAddressEntryText;

    [ObservableProperty]
    private WriteableBitmap? mouthBitmap;

    [ObservableProperty]
    private double rotation;

    [ObservableProperty]
    private bool enableCalibration;

    [ObservableProperty]
    private bool isVerticalFlip;

    [ObservableProperty]
    private bool isHorizontalFlip;

    public CamViewModel()
    {
        var settings = BabbleCore.Instance.Settings;
        cameraAddressEntryText = settings.Cam.CaptureSource;
        rotation = settings.Cam.RotationAngle;
        enableCalibration = settings.GeneralSettings.UseCalibration;
        isVerticalFlip = settings.Cam.GuiVerticalFlip;
        isHorizontalFlip = settings.Cam.GuiHorizontalFlip;
        
        BabbleCore.Instance.Settings.OnUpdate += Settings_OnUpdate;
    }

    private void Settings_OnUpdate(string obj)
    {
        // Handle a very niche edge case where the user starts up the application
        // With a previously correct URL, but it's not ready just yet
        // Don't send an update event!
        var settings = BabbleCore.Instance.Settings;
        cameraAddressEntryText = settings.Cam.CaptureSource;
    }
}
