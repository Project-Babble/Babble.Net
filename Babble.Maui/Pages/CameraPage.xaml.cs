using Babble.Core;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Runtime.InteropServices;

namespace Babble.Maui;

public partial class CameraPage : ContentPage
{
    public byte[] frame;
    private (int width, int height) dimensions;

    public CameraPage()
    {
        InitializeComponent();
        frame = Array.Empty<byte>();
        dimensions = (0, 0);
        CameraAddress.Text = BabbleCore.Instance.Settings.GetSetting<string>("capture_source");
    }

    public void OnPreviewCameraClicked(object sender, EventArgs args)
    {
        if (BabbleCore.Instance.GetImage(out var frame, out var dimensions))
        {
            MouthCanvasActivityView.IsRunning = true;
            this.frame = frame;
            this.dimensions = dimensions;
            MouthCanvasView.HeightRequest = dimensions.height;
            MouthCanvasView.WidthRequest = dimensions.width;
            MouthCanvasView.InvalidateSurface(); // Triggers the repaint of the canvas
        }
    }

    public void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        if (frame.Length == 0)
        {
            return;
        }

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var info = new SKImageInfo(dimensions.width, dimensions.height, SKColorType.Gray8);
        using var bitmap = new SKBitmap(info);

        //pin the frame to a array to prevent gc from moving it.
        var handle = GCHandle.Alloc(frame, GCHandleType.Pinned);
        try
        {
            var ptr = handle.AddrOfPinnedObject();
            bitmap.InstallPixels(info, ptr, info.RowBytes); //install pixels into the pointer manually to byte array
        }
        finally
        {
            handle.Free(); //ensure handle is free after use.
        }

        canvas.DrawBitmap(bitmap,new SKRect(0,0,dimensions.width,dimensions.height));
        MouthCanvasActivityView.IsRunning=false;
    }

    public void OnSaveAndRestartTrackingClicked(object sender, EventArgs args)
    {
        
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
        BabbleCore.Instance.Settings.UpdateSetting<int>("capture_source", ((Entry)sender).Text);
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