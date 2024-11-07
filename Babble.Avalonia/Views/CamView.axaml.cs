using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Babble.Avalonia.ReactiveObjects;
using Babble.Avalonia.Scripts.Enums;
using Babble.Core;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Babble.Avalonia;

public partial class CamView : UserControl
{
    private readonly CamViewModel _viewModel;
    private CamViewMode camViewMode = CamViewMode.Tracking;
    private bool ShouldDraw;

    public CamView()
    {
        InitializeComponent();

        _viewModel = new CamViewModel();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged;

        Loaded += CamView_OnLoaded;
        Unloaded += CamView_Unloaded;
        
        StartImageUpdates();
    }

    private void CamView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        ShouldDraw = true;
    }

    private void CamView_Unloaded(object? sender, RoutedEventArgs e)
    {
        ShouldDraw = false;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.MouthBitmap)) return;

        var settings = BabbleCore.Instance.Settings;
        switch (e.PropertyName)
        {
            case nameof(_viewModel.Rotation):
                settings.UpdateSetting<double>(nameof(settings.Cam.RotationAngle), _viewModel.Rotation.ToString());
                break;
            case nameof(_viewModel.EnableCalibration):
                settings.UpdateSetting<bool>(nameof(settings.GeneralSettings.UseCalibration), _viewModel.EnableCalibration.ToString());
                break;
            case nameof(_viewModel.IsVerticalFlip):
                settings.UpdateSetting<bool>(nameof(settings.Cam.GuiVerticalFlip), _viewModel.IsVerticalFlip.ToString());
                break;
            case nameof(_viewModel.IsHorizontalFlip):
                settings.UpdateSetting<bool>(nameof(settings.Cam.GuiHorizontalFlip), _viewModel.IsHorizontalFlip.ToString());
                break;
        }
        
        settings.Save();
    }

    private void StartImageUpdates()
    {
        // Start a timer to draw our face image
        // Note: In Debug mode this is slow af to update. Release isn't!
        DispatcherTimer drawTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        drawTimer.Tick += (s, e) => UpdateImage();
        drawTimer.Start();
    }

    private void UpdateImage()
    {
        byte[] image;
        (int width, int height) dims;
        bool valid;
        switch (camViewMode)
        {
            case CamViewMode.Tracking:
                valid = BabbleCore.Instance.GetImage(out image, out dims);
                break;
            case CamViewMode.Cropping:
                valid = BabbleCore.Instance.GetRawImage(out image, out dims);
                break;
            default:
                return;
        }

        if (valid && ShouldDraw)
        {
            if (dims.width == 0 || dims.height == 0)
            {
                MouthWindow.Width = 0;
                MouthWindow.Height = 0;
                Dispatcher.UIThread.Post(MouthWindow.InvalidateVisual, DispatcherPriority.Render);
                return;
            }

            if (_viewModel.MouthBitmap is null ||
                _viewModel.MouthBitmap.PixelSize.Width != dims.width ||
                _viewModel.MouthBitmap.PixelSize.Height != dims.height)
            {
                // On other versions of Avalonia there is support for Gray8 PixelFormats
                // But for the time being we'll roll our own converter
                _viewModel.MouthBitmap = new WriteableBitmap(
                    new PixelSize(dims.width, dims.height),
                    new Vector(96, 96),
                    PixelFormats.Gray8,
                    AlphaFormat.Opaque);
            }

            // BitmapConverter.WriteGrayscaleToWriteableBitmap(image, _viewModel.MouthBitmap, dims.width, dims.height);
            // https://github.com/AvaloniaUI/Avalonia/issues/9092
            using var frameBuffer = _viewModel.MouthBitmap.Lock();
            {
                Marshal.Copy(image, 0, frameBuffer.Address, image.Length);
            }

            if (MouthWindow.Width != dims.width || MouthWindow.Height != dims.height)
            {
                MouthWindow.Width = dims.width;
                MouthWindow.Height = dims.height;
            }

            Dispatcher.UIThread.Post(MouthWindow.InvalidateVisual, DispatcherPriority.Render);
        }
        else
        {
            MouthWindow.Width = 0;
            MouthWindow.Height = 0;
            Dispatcher.UIThread.Post(MouthWindow.InvalidateVisual, DispatcherPriority.Render);
        }
    }

    public void OnSaveAndRestartClicked(object sender, RoutedEventArgs args)
    {
        var settings = BabbleCore.Instance.Settings;
        settings.UpdateSetting<string>(nameof(settings.Cam.CaptureSource), _viewModel.CameraAddressEntryText);
        settings.Save();
    }

    public void OnTrackingModeClicked(object sender, RoutedEventArgs args)
    {
        camViewMode = CamViewMode.Tracking;
    }

    public void OnCroppingModeClicked(object sender, RoutedEventArgs args)
    {
        camViewMode = CamViewMode.Cropping;
    }

    public void StartCalibrationClicked(object sender, RoutedEventArgs args)
    {
        
    }

    public void StopCalibrationClicked(object sender, RoutedEventArgs args)
    {
        
    }
}