using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Babble.Avalonia.ReactiveObjects;
using Babble.Avalonia.Scripts;
using Babble.Avalonia.Scripts.Enums;
using Babble.Core;
using System.Runtime.InteropServices;

namespace Babble.Avalonia;

public partial class CamView : UserControl, IIsVisible
{
    public bool Visible { get; set; }

    private readonly CamViewModel _viewModel;
    private CamViewMode camViewMode = CamViewMode.Tracking;

    public CamView()
    {
        InitializeComponent();
        Loaded += CamView_OnLoaded;
        Unloaded += CamView_Unloaded;

        _viewModel = new CamViewModel();
        DataContext = _viewModel;

        this.FindControl<Slider>("RotationSlider")!.ValueChanged += RotationEntry_ValueChanged;
        this.FindControl<CheckBox>("EnableCalibration")!.IsCheckedChanged += EnableCalibration_Changed;
        this.FindControl<CheckBox>("VerticalFlip")!.IsCheckedChanged += VerticalFlip_Changed;
        this.FindControl<CheckBox>("HorizontalFlip")!.IsCheckedChanged += HorizontalFlip_Changed;

        StartImageUpdates();
    }

    private void CamView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Visible = true;
    }

    private void CamView_Unloaded(object? sender, RoutedEventArgs e)
    {
        Visible = false;
    }

    private void RotationEntry_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_viewModel.Rotation < 12)
        {
            BabbleCore.Instance.Settings.UpdateSetting<double>(
                nameof(BabbleCore.Instance.Settings.Cam.RotationAngle),
                "0");
        }
        else
        {
            BabbleCore.Instance.Settings.UpdateSetting<double>(
                nameof(BabbleCore.Instance.Settings.Cam.RotationAngle),
                _viewModel.Rotation.ToString());
        }

        BabbleCore.Instance.Settings.Save();
    }

    private void EnableCalibration_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        BabbleCore.Instance.Settings.UpdateSetting<bool>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.UseCalibration),
            checkBox.IsChecked.ToString()!);
        BabbleCore.Instance.Settings.Save();
    }

    private void VerticalFlip_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            BabbleCore.Instance.Settings.UpdateSetting<bool>(
                nameof(BabbleCore.Instance.Settings.Cam.GuiVerticalFlip),
                checkBox.IsChecked.ToString()!);
            BabbleCore.Instance.Settings.Save();
        }
    }

    private void HorizontalFlip_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            BabbleCore.Instance.Settings.UpdateSetting<bool>(
                nameof(BabbleCore.Instance.Settings.Cam.GuiHorizontalFlip),
                checkBox.IsChecked.ToString()!);
            BabbleCore.Instance.Settings.Save();
        }
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

        if (valid && Visible)
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
                _viewModel.MouthBitmap = new WriteableBitmap(
                    new PixelSize(dims.width, dims.height),
                    new Vector(96, 96),
                    PixelFormats.Gray8,
                    AlphaFormat.Opaque);
            }

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

    public void CameraAddressClicked(object? sender, RoutedEventArgs e)
    {
        BabbleCore.Instance.Settings.UpdateSetting<string>(
            nameof(BabbleCore.Instance.Settings.Cam.CaptureSource),
            _viewModel.CameraAddressEntryText);
        BabbleCore.Instance.Settings.Save();
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