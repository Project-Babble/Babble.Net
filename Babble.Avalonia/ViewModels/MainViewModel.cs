using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using System;
using Babble.Core;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia;
using Avalonia.Platform;
using DynamicData;

namespace Babble.Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
{
    public WriteableBitmap MouthBitmap { get; private set; }

    public MainViewModel()
    {
        MouthBitmap = new WriteableBitmap(new PixelSize(640, 480), new Vector(96, 96), PixelFormat.Rgb32);
        Task.Run(UpdateLipImage);
    }

    private async Task UpdateLipImage()
    {
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(500));
        while (await timer.WaitForNextTickAsync())
        {
            var hasHytes = BabbleCore.Instance.GetLipImage(out var image);
            if (hasHytes)
            {
                UpdateLipImage(image);
            }
        }
    }

    private unsafe void UpdateLipImage(byte[] image)
    {
        using (var frameBuffer = MouthBitmap.Lock())
        {
            Marshal.Copy(image, 0, frameBuffer.Address, image.Length);
        }
    }
}
