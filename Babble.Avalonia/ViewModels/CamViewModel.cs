using Avalonia.Media.Imaging;
using Babble.Core;
using Babble.Locale;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class CamViewModel : ObservableObject, ILocalizable
{
    [ObservableProperty]
    private string cameraAddressEntryText;

    [ObservableProperty]
    private WriteableBitmap? mouthBitmap;

    [ObservableProperty]
    private float rotation;

    [ObservableProperty]
    private bool enableCalibration;

    [ObservableProperty]
    private bool isVerticalFlip;

    [ObservableProperty]
    private bool isHorizontalFlip;

    public string CameraAddressText { get; private set; }
    public string CameraAddressTextTooltip { get; private set; }
    public string SaveAndRestartTrackingText { get; private set; }
    public string SaveAndRestartTrackingTextTooltip { get; private set; }
    public string TrackingModeText { get; private set; }
    public string TrackingModeTextTooltip { get; private set; }
    public string CroppingModeText { get; private set; }
    public string CroppingModeTextTooltip { get; private set; }
    public string RotationText { get; private set; }
    public string RotationTextTooltip { get; private set; }
    public string StartCalibrationText { get; private set; }
    public string StartCalibrationTextTooltip { get; private set; }
    public string StopCalibrationText { get; private set; }
    public string StopCalibrationTextTooltip { get; private set; }
    public string EnableCalibrationText { get; private set; }
    public string EnableCalibrationTextTooltip { get; private set; }
    public string ModeText { get; private set; }
    public string ModeTextTooltip { get; private set; }
    public string VerticalFlipText { get; private set; }
    public string VerticalFlipTextTooltip { get; private set; }
    public string HorizontalFlipText { get; private set; }
    public string HorizontalFlipTextTooltip { get; private set; }

    public CamViewModel()
    {
        var settings = BabbleCore.Instance.Settings;
        cameraAddressEntryText = settings.Cam.CaptureSource;
        rotation = settings.Cam.RotationAngle;
        enableCalibration = settings.GeneralSettings.UseCalibration;
        isVerticalFlip = settings.Cam.GuiVerticalFlip;
        isHorizontalFlip = settings.Cam.GuiHorizontalFlip;

        Localize();
        LocaleManager.OnLocaleChanged += Localize;
    }

    public void Localize()
    {
        CameraAddressText = LocaleManager.Instance["camera.address"];
        CameraAddressTextTooltip = LocaleManager.Instance["camera.addressTooltip"];
        SaveAndRestartTrackingText = LocaleManager.Instance["camera.saveAndRestartTracking"];
        SaveAndRestartTrackingTextTooltip = LocaleManager.Instance["camera.saveAndRestartTrackingTooltip"];
        TrackingModeText = LocaleManager.Instance["camera.trackingMode"];
        TrackingModeTextTooltip = LocaleManager.Instance["camera.trackingModeTooltip"];
        CroppingModeText = LocaleManager.Instance["camera.croppingMode"];
        CroppingModeTextTooltip = LocaleManager.Instance["camera.croppingModeTooltip"];
        RotationText = LocaleManager.Instance["camera.rotation"];
        RotationTextTooltip = LocaleManager.Instance["camera.rotationTooltip"];
        StartCalibrationText = LocaleManager.Instance["camera.startCalibration"];
        StartCalibrationTextTooltip = LocaleManager.Instance["camera.startCalibrationTooltip"];
        StopCalibrationText = LocaleManager.Instance["camera.stopCalibration"];
        StopCalibrationTextTooltip = LocaleManager.Instance["camera.stopCalibrationTooltip"];
        EnableCalibrationText = LocaleManager.Instance["camera.enableCalibration"];
        EnableCalibrationTextTooltip = LocaleManager.Instance["camera.enableCalibrationTooltip"];
        ModeText = LocaleManager.Instance["camera.mode"];
        ModeTextTooltip = LocaleManager.Instance["camera.modeTooltip"];
        VerticalFlipText = LocaleManager.Instance["camera.verticalFlip"];
        VerticalFlipTextTooltip = LocaleManager.Instance["camera.verticalFlipTooltip"];
        HorizontalFlipText = LocaleManager.Instance["camera.horizontalFlip"];
        HorizontalFlipTextTooltip = LocaleManager.Instance["camera.horizontalFlipTooltip"];
    }
}
