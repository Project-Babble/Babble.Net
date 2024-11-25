using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Babble.Core.Scripts.EmguCV;

/// <summary>
/// Handles the chain of Mat operations with proper resource disposal
/// </summary>
public class MatProcessingChain : IDisposable
{
    private readonly List<Mat> _matsToDispose = new();
    private Mat? _currentMat;

    public Mat Result => _currentMat ?? throw new InvalidOperationException("No processing has been started");
    
    public MatProcessingChain StartWith(Mat frameData, (int width, int height) dimensions)
    {
        _currentMat = new Mat(dimensions.width, dimensions.height, DepthType.Cv8U, 1);

        // Previously, Captures always passed in a "Rgb888x" image, 1 byte per pixel.
        // This was problematic for a couple of reasons. It mainly caused confusion, so 
        // Instead we do the Mat conversion here because we aren't sadists.

        // _matsToDispose.Add(_currentMat);
        // var matBytes = _currentMat.DataPointer;
        // Marshal.Copy(frameData, 0, matBytes, frameData.Length);

        if (frameData.NumberOfChannels != 3)
        {
            throw new InvalidOperationException("Input Mat must be in BGR format!");
        }

        var grayMat = new Mat();
        _matsToDispose.Add(grayMat);
        CvInvoke.CvtColor(frameData, grayMat, ColorConversion.Bgr2Gray);

        _currentMat = grayMat;
        return this;
    }

    public MatProcessingChain UseRedChannel(bool useRedChannel)
    {
        EnsureCurrentMat();

        if (useRedChannel)
        {
            // Firstly, we need to determine if 

            var newMat = new Mat();
            _matsToDispose.Add(newMat);

            // Split the channels
            using var channels = new VectorOfMat();
            CvInvoke.Split(_currentMat, channels);

            // Get the red channel (which is actually index 2 in BGR format)
            newMat = channels[2].Clone();

            // Add the cloned red channel to disposal list
            _matsToDispose.Add(newMat);

            // Assign this mat as current
            _currentMat = newMat;
        }
        
        return this;
    }

    public MatProcessingChain Rotate(double angle)
    {
        EnsureCurrentMat();

        // Skip if no rotation needed
        if (angle == 0 || angle % 360 == 0)
        {
            return this;
        }

        var newMat = new Mat();
        _matsToDispose.Add(newMat);

        // Get the rotation matrix
        Point center = new(_currentMat.Cols / 2, _currentMat.Rows / 2);
        using var rotationMatrix = new Mat(2, 3, DepthType.Cv64F, 1);
        CvInvoke.GetRotationMatrix2D(center, angle, 1.0, rotationMatrix);

        // Get the new image size after rotation
        double radians = angle * Math.PI / 180.0;
        double sin = Math.Abs(Math.Sin(radians));
        double cos = Math.Abs(Math.Cos(radians));
        int newWidth = (int)((_currentMat.Cols * cos) + (_currentMat.Rows * sin));
        int newHeight = (int)((_currentMat.Cols * sin) + (_currentMat.Rows * cos));

        // Adjust the rotation matrix
        double[] matrixData = new double[6];
        Marshal.Copy(rotationMatrix.DataPointer, matrixData, 0, 6);

        // Update translation components
        matrixData[2] += (newWidth - _currentMat.Cols) / 2.0;  // tx
        matrixData[5] += (newHeight - _currentMat.Rows) / 2.0; // ty

        // Copy back to rotation matrix
        Marshal.Copy(matrixData, 0, rotationMatrix.DataPointer, 6);

        // Apply the rotation
        CvInvoke.WarpAffine(_currentMat, newMat, rotationMatrix, new Size(newWidth, newHeight));
        _currentMat = newMat;

        return this;
    }

    public MatProcessingChain Crop(int x, int y, int width, int height)
    {
        // Don't crop if we're using defaults
        if (width == 0 || height == 0)
        {
            return this;
        }

        // Don't crop if we have the entire frame selected
        if (x == 0 && y == 0 && width == _currentMat.Width && height == _currentMat.Height)
        {
            return this;
        }

        EnsureCurrentMat();

        // Validate crop parameters
        if (x > _currentMat.Rows ||             // Is the starting x pixel out of range?
            width > _currentMat.Rows ||         // Is the starting x crop wider than the image input
            y > _currentMat.Cols ||             // Is the starting y pixel out of range?
            height > _currentMat.Cols ||        // Is the starting y crop wider than the image input
            x + width > _currentMat.Rows ||     // Is the starting x pixel + the x crop out of range?
            y + height > _currentMat.Cols)      // Is the starting y pixel + the y crop wider than the image input
        {
            throw new ArgumentException($"Invalid crop parameters: x={x}, y={y}, width={width}, height={height}. " +
                                        $"Image size is {_currentMat.Cols}x{_currentMat.Rows}");
        }

        var rect = new Rectangle(x, y, width, height);
        var newMat = new Mat(_currentMat, rect);
        _matsToDispose.Add(newMat);
        _currentMat = newMat;

        return this;
    }

    public MatProcessingChain Resize(Size size)
    {
        EnsureCurrentMat();
        var newMat = new Mat();
        _matsToDispose.Add(newMat);
        CvInvoke.Resize(_currentMat, newMat, size);
        _currentMat = newMat;
        return this;
    }
    
    public MatProcessingChain ApplyFlip(bool flip, FlipType flipType)
    {
        EnsureCurrentMat();
        if (flip)
        {
            var newMat = new Mat();
            _matsToDispose.Add(newMat);
            CvInvoke.Flip(_currentMat, newMat, flipType);
            _currentMat = newMat;
        }
        return this;
    }

    private void EnsureCurrentMat()
    {
        if (_currentMat == null)
        {
            throw new InvalidOperationException("Processing chain not properly initialized");
        }
    }

    public void Dispose()
    {
        foreach (var mat in _matsToDispose)
        {
            mat.Dispose();
        }
        _matsToDispose.Clear();
    }
}
