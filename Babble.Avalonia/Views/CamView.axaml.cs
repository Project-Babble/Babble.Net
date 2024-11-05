using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Babble.Avalonia.ReactiveObjects;
using Babble.Core;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Babble.Avalonia;

public partial class CamView : UserControl
{
    private readonly CamViewModel _viewModel;
    private WriteableBitmap? _currentBitmap;

    public CamView()
    {
        InitializeComponent();

        _viewModel = new CamViewModel();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged;

        // Start update loop immediately
        StartImageUpdates();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.MouthBitmap)) return;

        var settings = BabbleCore.Instance.Settings;
        switch (e.PropertyName)
        {
            case nameof(_viewModel.CameraAddressEntryText):
                BabbleCore.Instance.Settings.UpdateSetting<string>(e.PropertyName, _viewModel.CameraAddressEntryText);
                break;
            case nameof(_viewModel.Rotation):
                BabbleCore.Instance.Settings.UpdateSetting<double>(nameof(settings.Cam.RotationAngle), _viewModel.Rotation.ToString());
                break;
            case nameof(_viewModel.EnableCalibration):
                BabbleCore.Instance.Settings.UpdateSetting<bool>(nameof(settings.GeneralSettings.UseCalibration), _viewModel.EnableCalibration.ToString());
                break;
            case nameof(_viewModel.IsVerticalFlip):
                BabbleCore.Instance.Settings.UpdateSetting<bool>(nameof(settings.Cam.GuiVerticalFlip), _viewModel.IsVerticalFlip.ToString());
                break;
            case nameof(_viewModel.HorizontalFlipText):
                BabbleCore.Instance.Settings.UpdateSetting<bool>(nameof(settings.Cam.GuiHorizontalFlip), _viewModel.IsHorizontalFlip.ToString());
                break;

        }
        
        settings.Save();
    }

    private void StartImageUpdates()
    {
        // Run the update loop at 30fps
        DispatcherTimer timer = new()
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 30.0) // 30fps
        };

        timer.Tick += async (s, e) => await UpdateImageAsync();
        timer.Start();
    }

    private async Task UpdateImageAsync()
    {
        if (!BabbleCore.Instance.GetImage(out var image, out var dims))
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_currentBitmap is null ||
                _currentBitmap.PixelSize.Width != dims.width ||
                _currentBitmap.PixelSize.Height != dims.height)
            {
                // On other versions of Avalonia there is support for Gray8 PixelFormats
                // But for the time being we'll roll our own converter
                _currentBitmap = new WriteableBitmap(
                    new PixelSize(dims.width, dims.height),
                    new Vector(96, 96),
                    PixelFormat.Rgba8888,
                    AlphaFormat.Unpremul);
            }

            BitmapConverter.WriteGrayscaleToWriteableBitmap(image, _currentBitmap, dims.width, dims.height);

            // Force update by creating a new reference
            _viewModel.MouthBitmap = null;
            _viewModel.MouthBitmap = _currentBitmap;

            // Update control dimensions if needed
            if (MouthWindow.Width != dims.width || MouthWindow.Height != dims.height)
            {
                MouthWindow.Width = dims.width;
                MouthWindow.Height = dims.height;
            }

            // Ensure the image control is invalidated
            MouthWindow.InvalidateVisual();
        }, DispatcherPriority.Render);
    }
}