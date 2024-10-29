using Babble.Core;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace Babble.Maui;

public partial class CameraPage : ContentPage
{
    private byte[] frame;
    private (int width, int height) dimensions;
    private CancellationTokenSource _cancellationTokenSource;
    public CameraPage()
    {
        InitializeComponent();
        frame = Array.Empty<byte>();
        dimensions = (0, 0);
        CameraAddress.Text = BabbleCore.Instance.Settings.GetSetting<string>("capture_source");
    }

    public async void OnPreviewCameraClicked(object sender, EventArgs args)
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
            MouthCanvasActivityView.IsRunning = false;
        }
        else
        {
                _cancellationTokenSource = new CancellationTokenSource();
            MouthCanvasActivityView.IsRunning = true;
            await StartVideoFeed(_cancellationTokenSource.Token);
        }
    }

    private async Task StartVideoFeed(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {

            await Task.Run(() =>
            {
                if (BabbleCore.Instance.GetImage(out var retrivedFrame, out var retrievedDimensions))
                {
                    frame = retrivedFrame;
                    dimensions = retrievedDimensions;
                }
            });

            Dispatcher.Dispatch(() =>
            {
                if (dimensions.width > 0 && dimensions.height > 0)
                {
                    MouthCanvasView.HeightRequest = dimensions.height;
                    MouthCanvasView.WidthRequest = dimensions.width;
                    MouthCanvasView.InvalidateSurface();
                }
            });

            await Task.Delay(30);
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
        // Placeholder for tracking restart logic
    }

    public void OnTrackingModeClicked(object sender, EventArgs args)
    {
        // Placeholder for tracking mode logic
    }

    public void OnCroppingModeClicked(object sender, EventArgs args)
    {
        // Placeholder for cropping mode logic
    }

    public void OnStartCalibrationClicked(object sender, EventArgs args)
    {
        // Placeholder for start calibration logic
    }

    public void OnStopCalibrationClicked(object sender, EventArgs args)
    {
        // Placeholder for stop calibration logic
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