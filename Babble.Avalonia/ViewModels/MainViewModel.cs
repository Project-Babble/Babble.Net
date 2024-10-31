using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Babble.Core;
using System.Runtime.InteropServices;

namespace Babble.Avalonia.ViewModels;

public class MainViewModel
{
    public static MainViewModel Instance { get; private set; }

    public WriteableBitmap? MouthBitmap { get; private set; }

    private byte[] rgbaBuffer;

    public MainViewModel()
    {
        if (Instance is not null) return;

        const int width = 640;
        const int height = 480;
        MouthBitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Rgba8888);
        rgbaBuffer = new byte[width * height * 4];
        UpdateLipImage();
        Instance = this;
    }

    public void UpdateLipImage()
    {
        var hasHytes = BabbleCore.Instance.GetImage(out var image, out var dimensions);
        if (hasHytes)
        {
            UpdateFrameBuffer(image);
        }
    }

    private void UpdateFrameBuffer(byte[] rgbImage)
    {
        int rgbLength = rgbImage.Length;
        int rgbaLength = (rgbLength / 3) * 4;

        // Convert BGR to RGBA
        for (int i = 0, j = 0; i < rgbLength; i += 3, j += 4)
        {
            rgbaBuffer[j] = rgbImage[i + 2];     // B -> R
            rgbaBuffer[j + 1] = rgbImage[i + 1]; // G -> G
            rgbaBuffer[j + 2] = rgbImage[i + 1]; // R -> B
            rgbaBuffer[j + 3] = 255;             // A (fully opaque)
        }

        // Copy the converted buffer to the bitmap
        using (var frameBuffer = MouthBitmap.Lock())
        {
            Marshal.Copy(rgbaBuffer, 0, frameBuffer.Address, rgbaLength);
        }
    }
}
