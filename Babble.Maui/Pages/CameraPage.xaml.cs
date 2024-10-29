using Babble.Core;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Babble.Maui;

public partial class CameraPage : ContentPage
{
    public byte[] frame;
    private (int width, int height) dimensions;

    public CameraPage()
    {
        InitializeComponent();
        CameraAddress.Text = BabbleCore.Instance.Settings.GetSetting<string>("capture_source");
    }

    public void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        if (frame is null)
        {
            return;
        }

        if (frame.Length == 0)
        {
            return;
        }

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var bitmap = new SKBitmap(dimensions.width, dimensions.height, SKColorType.Rgb888x, SKAlphaType.Opaque);

        int i = 0;
        for (int y = 0; y < dimensions.height; y++)
        {
            for (int x = 0; x < dimensions.width; x++)
            {
                bitmap.SetPixel(x, y, new SKColor(frame[i], frame[i], frame[i]));
                i++;
            }
        }

        canvas.DrawBitmap(bitmap, new SKRect(0, 0, dimensions.width, dimensions.height));
        MouthCanvasActivityView.IsRunning = false;
    }

    public async void OnPreviewCameraClicked(object sender, EventArgs args)
    {
        if (BabbleCore.Instance.GetLipImage(out var frame, out var dimensions))
        {
            MouthCanvasActivityView.IsRunning = true;
            this.frame = frame;
            this.dimensions = dimensions;
            MouthCanvasView.InvalidateSurface(); // Triggers a repaint of the canvas
        }

    }

    public async void OnCameraAddressChanged(object sender, TextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(args.OldTextValue)) return;
        if (args.OldTextValue != args.NewTextValue)
            BabbleCore.Instance.Settings.UpdateSetting<int>("capture_source", args.NewTextValue);
    }


    public async void OnSaveAndRestartTrackingClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnTrackingModeClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnCroppingModeClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnSliderRotationChanged(object sender, EventArgs args)
    {
        
    }

    public async void OnStartCalibrationClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnStopCalibrationClicked(object sender, EventArgs args)
    {
        
    }

    public async void OnEnableCalibrationToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("use_calibration", args.Value.ToString());
    }

    public async void OnVerticalFlipToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_vertical_flip", args.Value.ToString());
    }

    public async void OnHorizontalFlipToggled(object sender, CheckedChangedEventArgs args)
    {
        BabbleCore.Instance.Settings.UpdateSetting<bool>("gui_horizontal_flip", args.Value.ToString());
    }
}