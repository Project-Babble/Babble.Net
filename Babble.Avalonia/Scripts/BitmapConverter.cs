using Avalonia.Media.Imaging;
using System.Runtime.InteropServices;

public static class BitmapConverter
{
    public static void WriteGrayscaleToWriteableBitmap(byte[] sourceData, WriteableBitmap targetBitmap, int width, int height)
    {
        // Calculate strides
        int sourceStride = width; // 8-bit = 1 byte per pixel
        int targetStride = width * 4; // 32-bit RGBA = 4 bytes per pixel

        // Create buffer for target pixels (32-bit RGBA)
        byte[] targetBuffer = new byte[targetStride * height];

        using (var lockedBitmap = targetBitmap.Lock())
        {
            // Convert each grayscale value to RGBA
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get the grayscale value
                    byte grayValue = sourceData[y * sourceStride + x];

                    // Calculate target position (32BPP RGBA)
                    int targetIndex = (y * targetStride) + (x * 4);

                    // Copy the grayscale value to all RGB channels
                    targetBuffer[targetIndex] = grayValue;     // R
                    targetBuffer[targetIndex + 1] = grayValue; // G
                    targetBuffer[targetIndex + 2] = grayValue; // B
                    targetBuffer[targetIndex + 3] = 255;       // A (fully opaque)
                }
            }

            // Copy the buffer to the bitmap
            Marshal.Copy(targetBuffer, 0, lockedBitmap.Address, targetBuffer.Length);
        }
    }
}