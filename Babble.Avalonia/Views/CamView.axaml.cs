using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia;
using Babble.Avalonia.ReactiveObjects;
using Babble.Core;

namespace Babble.Avalonia
{
    public partial class CamView : UserControl
    {
        private readonly CamViewModel _viewModel;
        private WriteableBitmap? _currentBitmap;

        public CamView()
        {
            InitializeComponent();

            _viewModel = new CamViewModel();
            DataContext = _viewModel;

            // Start update loop immediately
            StartImageUpdates();
        }

        private void StartImageUpdates()
        {
            // Run the update loop at 30fps
            DispatcherTimer timer = new DispatcherTimer
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
}