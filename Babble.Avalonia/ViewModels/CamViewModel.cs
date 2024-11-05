using Avalonia.Media.Imaging;
using Babble.Locale;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class CamViewModel : ObservableObject
{
    [ObservableProperty]
    private WriteableBitmap? _mouthBitmap;

    public string CameraAddressText => LocaleManager.Instance["general.header"];

    public string SaveAndRestartTrackingText => LocaleManager.Instance["camera.saveAndRestartTracking"];

    public string TrackingModeText => LocaleManager.Instance["camera.trackingMode"];

    public string CroppingModeText => LocaleManager.Instance["camera.croppingMode"];

    public string RotationText => LocaleManager.Instance["camera.rotation"];

    public string StartCalibrationText => LocaleManager.Instance["camera.startCalibration"];

    public string StopCalibrationText => LocaleManager.Instance["camera.stopCalibration"];

    public string EnableCalibrationText => LocaleManager.Instance["camera.enableCalibration"];

    public string ModeText => LocaleManager.Instance["camera.mode"];

    public string VerticalFlipText => LocaleManager.Instance["camera.verticalFlip"];

    public string HorizontalFlipText => LocaleManager.Instance["camera.horizontalFlip"];

    public CamViewModel()
    {
        
    }
}
