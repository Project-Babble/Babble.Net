using Babble.Core;
using Babble.Maui.Locale;
using Microsoft.Maui.Media;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Runtime.InteropServices;

namespace Babble.Maui;

public partial class CameraPage : ContentPage, ILocalizable
{
    private byte[] frame = Array.Empty<byte>();
    private (int width, int height) dimensions = (0, 0);

    public CameraPage()
    {
        InitializeComponent();

        var settings = BabbleCore.Instance.Settings;
        CameraAddress.Text = settings.GetSetting<string>("capture_source");

        Localize();
        LocaleManager.OnLocaleChanged += Localize;
    }

    public void Localize()
    {
        LocaleManager.SetLocalizedText(CameraAddressText, "camera.cameraAddress", "camera.cameraAddressTooltip");
        SaveRestart.Text = LocaleManager.Instance["saveAndRestartTracking"];
    }

    public void OnPreviewCameraClicked(object sender, EventArgs args)
    {
        if (BabbleCore.Instance.GetImage(out var retrivedFrame, out var retrievedDimensions))
        {

            frame = retrivedFrame;
            dimensions = retrievedDimensions;

            if (dimensions.width > 0 && dimensions.height > 0)
            {
                MouthCanvasActivityView.IsRunning = true;
#if ANDROID || IOS
                MouthCanvasView.HeightRequest = 256;
                MouthCanvasView.WidthRequest = 256;
#else
                MouthCanvasView.HeightRequest = dimensions.height;
                MouthCanvasView.WidthRequest = dimensions.width;
#endif
                MouthCanvasView.InvalidateSurface();
                MouthCanvasActivityView.IsRunning = false;
            }  
        }        
    }

    public void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        if (frame == null || frame.Length == 0 || dimensions.width == 0 || dimensions.height == 0)
        {
            return;
        }

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var info = new SKImageInfo(dimensions.width, dimensions.height, SKColorType.Gray8);
        using var bitmap = new SKBitmap(info);

        // Pin the frame to an array to prevent GC from moving it.
        var handle = GCHandle.Alloc(frame, GCHandleType.Pinned);
        try
        {
            var ptr = handle.AddrOfPinnedObject();
            bitmap.InstallPixels(info, ptr, info.RowBytes); // Install pixels into the bitmap
        }
        finally
        {
            handle.Free(); // Ensure handle is free after use
        }

        // Draw the bitmap directly on the canvas on the UI thread
        canvas.DrawBitmap(bitmap, new SKRect(0, 0, dimensions.width, dimensions.height));
    }

    public void OnSaveAndRestartTrackingClicked(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>("capture_source", CameraAddress.Text);
    }

    public void OnTrackingModeClicked(object sender, EventArgs args)
    {
        
    }

    public void OnCroppingModeClicked(object sender, EventArgs args)
    {
        
    }

    public void OnStartCalibrationClicked(object sender, EventArgs args)
    {
        
    }

    public void OnStopCalibrationClicked(object sender, EventArgs args)
    {
        
    }

    public void OnCameraAddressChanged(object sender, EventArgs args)
    {
        
    }

    public void OnSliderRotationChanged(object sender, EventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("rotation_angle", ((Entry)sender).Text);
    }

    public void OnEnableCalibrationToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("use_calibration", args.Value.ToString());
    }

    public void OnVerticalFlipToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_vertical_flip", args.Value.ToString());
    }

    public void OnHorizontalFlipToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_horizontal_flip", args.Value.ToString());
    }
}