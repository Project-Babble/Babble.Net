using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Babble.Avalonia.ViewModels;
using Babble.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class CamViewModel : ViewModelBase
{
    [ObservableProperty]
    private string cameraAddressEntryText;

    [ObservableProperty]
    private WriteableBitmap mouthBitmap;

    [ObservableProperty]
    private Canvas overlayCanvas;

    [ObservableProperty]
    private Rect overlayRectangle;

    [ObservableProperty]
    private int overlayRectangleCanvasX;

    [ObservableProperty]
    private int overlayRectangleCanvasY;

    [ObservableProperty]
    private double rotation;

    [ObservableProperty]
    private string perf;

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
        overlayRectangleCanvasX = settings.Cam.RoiWindowX;
        overlayRectangleCanvasY = settings.Cam.RoiWindowY;
        overlayRectangle = new Rect(
            overlayRectangleCanvasX,
            overlayRectangleCanvasY,
            settings.Cam.RoiWindowW,
            settings.Cam.RoiWindowH);

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
