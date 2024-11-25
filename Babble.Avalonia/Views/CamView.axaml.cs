using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
    public bool Visible { get => _isVisible; }
    private bool _isVisible;

    private readonly CamViewModel _viewModel;
    private CamViewMode camViewMode = CamViewMode.Tracking;
    private Point? cropStartPoint;
    private Rect? cropRectangle;
    private bool isCropping;

    public CamView()
    {
        InitializeComponent();
        Loaded += CamView_OnLoaded;
        Unloaded += CamView_Unloaded;
        MouthWindow.PointerPressed += OnPointerPressed;
        MouthWindow.PointerMoved += OnPointerMoved;
        MouthWindow.PointerReleased += OnPointerReleased;
        //MouthWindow.Tapped += OnTapped;
        //MouthWindow.DoubleTapped += OnDoubleTapped;

        TrackingModeButton.IsChecked = true;
        CroppingModeButton.IsChecked = false;

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
        _isVisible = true;
    }

    private void CamView_Unloaded(object? sender, RoutedEventArgs e)
    {
        _isVisible = false;
    }


    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        OnContactEnter(e.GetPosition(MouthWindow));
    }

    //private void OnTapped(object? sender, TappedEventArgs e)
    //{
    //    OnContactEnter(e.GetPosition(MouthWindow));
    //}

    private void OnContactEnter(Point position)
    {
        if (camViewMode != CamViewMode.Cropping) return;

        cropStartPoint = position;
        cropRectangle = new Rect(position.X, position.Y, 0, 0);
        isCropping = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        OnContactMoved(e.GetPosition(MouthWindow));
    }

    //private void OnDoubleTapped(object? sender, TappedEventArgs e)
    //{
    //    OnContactMoved(e.GetPosition(MouthWindow));
    //    OnPointerReleased(null, null);
    //}

    private void OnContactMoved(Point position)
    {
        if (!isCropping || cropStartPoint is null) return;

        var x = Math.Min(cropStartPoint.Value.X, position.X);
        var y = Math.Min(cropStartPoint.Value.Y, position.Y);
        var clampedWidth = Math.Clamp(Math.Abs(cropStartPoint.Value.X - position.X), 0, _viewModel.MouthBitmap.Size.Width - cropStartPoint.Value.X);
        var clampedHeight = Math.Clamp(Math.Abs(cropStartPoint.Value.Y - position.Y), 0, _viewModel.MouthBitmap.Size.Height - cropStartPoint.Value.Y);

        cropRectangle = new Rect(x, y, clampedWidth, clampedHeight);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!isCropping) return;

        isCropping = false;

        if (cropStartPoint.HasValue)
        {
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.Cam.RoiWindowX),
                ((int)cropStartPoint.Value.X).ToString());
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.Cam.RoiWindowY),
                ((int)cropStartPoint.Value.Y).ToString());

            BabbleCore.Instance.Settings.Save();
        }

        if (cropRectangle.HasValue)
        {
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.Cam.RoiWindowW),
                ((int)cropRectangle.Value.Width).ToString());
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.Cam.RoiWindowH),
                ((int)cropRectangle.Value.Height).ToString());

            BabbleCore.Instance.Settings.Save();
        }
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
        RectangleWindow.Stroke = Brushes.Red;
        RectangleWindow.StrokeThickness = 2;

        DispatcherTimer drawTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        drawTimer.Tick += (s, e) => UpdateImage();
        drawTimer.Start();
    }

    private void UpdateImage()
    {
        var isCroppingModeUIVisible = camViewMode == CamViewMode.Cropping;
        RectangleWindow.IsVisible = isCroppingModeUIVisible;
        SelectEntireFrame.IsVisible = isCroppingModeUIVisible;

        if (!BabbleCore.Instance.IsRunning) return;

        bool valid;
        byte[] image;
        (int width, int height) dims;
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
            if (dims.width == 0 || dims.height == 0 || image is null)
            {
                MouthWindow.Width = 0;
                MouthWindow.Height = 0;
                CanvasWindow.Width = 0;
                CanvasWindow.Height = 0;
                RectangleWindow.Width = 0;
                RectangleWindow.Height = 0;
                Dispatcher.UIThread.Post(MouthWindow.InvalidateVisual, DispatcherPriority.Render);
                Dispatcher.UIThread.Post(CanvasWindow.InvalidateVisual, DispatcherPriority.Render);
                Dispatcher.UIThread.Post(RectangleWindow.InvalidateVisual, DispatcherPriority.Render);
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
                CanvasWindow.Width = dims.width;
                CanvasWindow.Height = dims.height;
            }

            if (cropRectangle.HasValue)
            {
                RectangleWindow.Width = cropRectangle.Value.Width;
                RectangleWindow.Height = cropRectangle.Value.Height;
            }

            if (cropStartPoint.HasValue)
            {
                _viewModel.OverlayRectangleCanvasX = ((int)cropStartPoint.Value.X);
                _viewModel.OverlayRectangleCanvasY = ((int)cropStartPoint.Value.Y);
            }
        }
        else
        {
            MouthWindow.Width = 0;
            MouthWindow.Height = 0;
            CanvasWindow.Width = 0;
            CanvasWindow.Height = 0;
            RectangleWindow.Width = 0;
            RectangleWindow.Height = 0;   
        }

        Dispatcher.UIThread.Post(MouthWindow.InvalidateVisual, DispatcherPriority.Render);
        Dispatcher.UIThread.Post(CanvasWindow.InvalidateVisual, DispatcherPriority.Render);
        Dispatcher.UIThread.Post(RectangleWindow.InvalidateVisual, DispatcherPriority.Render);
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
        isCropping = false;
        OnPointerReleased(null, null); // Close and save any open crops
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

    public void SelectEntireFrameClicked(object sender, RoutedEventArgs args)
    {
        cropStartPoint = new Point(0, 0);
        cropRectangle = new Rect(0, 0, _viewModel.MouthBitmap.Size.Width, _viewModel.MouthBitmap.Size.Height);

        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.Cam.RoiWindowX),
            ((int)cropStartPoint.Value.X).ToString());
        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.Cam.RoiWindowY),
            ((int)cropStartPoint.Value.Y).ToString());

        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.Cam.RoiWindowW),
            ((int)cropRectangle.Value.Width).ToString());
        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.Cam.RoiWindowH),
            ((int)cropRectangle.Value.Height).ToString());

        BabbleCore.Instance.Settings.Save();

    }
}