using OpenCvSharp;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Manages what Captures are allowed to run on what platforms, as well as their Urls, etc.
/// </summary>
public abstract class PlatformConnector
{
    /// <summary>
    /// The path to where the "data" lies
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// A Platform may have many Capture sources, but only one may ever be active at a time.
    /// This represents the current (and a valid) Capture source for this Platform
    /// </summary>
    public Capture? Capture { get; set; }

    private uint _lastFrameCount = 0;

    public PlatformConnector(string Url)
    {
        this.Url = Url;
        Capture = null;
    }

    /// <summary>
    /// Initializes a Platform Connector
    /// </summary>
    public virtual void Initialize()
    {
        Capture = null;
    }

    /// <summary>
    /// Converts Capture.Frame into something Babble can understand
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public unsafe float[] ExtractFrameData(Size size)
    {
        if (Capture is null)
        {
            return Array.Empty<float>();
        }
        if (!Capture.IsReady)
        {
            return Array.Empty<float>();
        }
        if (Capture.RawMat is null)
        {
            return Array.Empty<float>();
        }
        if (Capture.RawMat.DataPointer == null) // Non-copying version of Capture.RawFrame.GetRawData().Length is null
        {
            return Array.Empty<float>();
        }
        if (Capture.FrameCount == _lastFrameCount)
        {
            return Array.Empty<float>();
        }

        _lastFrameCount = Capture.FrameCount;

        using Mat resultMat = TransformRawImage(size);

        using var finalMat = new Mat();
        resultMat.ConvertTo(finalMat, MatType.CV_32F);

        // Convert to float array and normalize
        // float[] floatArray = new float[finalMat.Height * finalMat.Width];
        finalMat.GetArray<float>(out var floatArray);

        // Normalize pixel values to [0, 1]?
        for (int i = 0; i < floatArray.Length; i++)
        {
            floatArray[i] /= 255f;
        }

        return floatArray;
    }
    
    public unsafe Mat TransformRawImage(Size size)
    {
        // If this method is called from above, then the below checks don't apply
        // We need this in case we poll from Babble.Core.cs, in which the developer
        // Just wants the frame data, not expression data

        var emptyMat = Mat.Zeros(0, 0);
        if (Capture is null)
        {
            return emptyMat;
        }
        if (!Capture.IsReady)
        {
            return emptyMat;
        }
        if (Capture.RawMat is null)
        {
            return emptyMat;
        }
        if (Capture.RawMat.DataPointer == null) // Non-copying version of Capture.RawFrame.GetRawData().Length is null
        {
            return emptyMat;
        }

        var settings = BabbleCore.Instance.Settings;
        var camSettings = settings.Cam;
        var roiX = camSettings.RoiWindowX;
        var roiY = camSettings.RoiWindowY;
        var roiWidth = camSettings.RoiWindowW;
        var roiHeight = camSettings.RoiWindowH;
        var rotationRadians = camSettings.RotationAngle * Math.PI / 180.0;
        var useRedChannel = settings.GeneralSettings.GuiUseRedChannel;

        // Check to see if the user's supplied crop is too large. IE, they were using a higher resolution camera, but they switched to a smaller one
        if (roiX > Capture.RawMat.Width)
        {
            roiX = 0;
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.Cam.RoiWindowX),
                roiX.ToString());
            BabbleCore.Instance.Settings.Save();
        }
        if (roiY > Capture.RawMat.Height)
        {
            roiY = 0;
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.Cam.RoiWindowY),
                roiY.ToString());
            BabbleCore.Instance.Settings.Save();
        }
        if (roiWidth > Capture.RawMat.Width)
        {
            roiWidth = Math.Clamp(roiWidth, 0, Capture.RawMat.Width);
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.Cam.RoiWindowW),
                roiWidth.ToString());
            BabbleCore.Instance.Settings.Save();
        }
        if (roiHeight > Capture.RawMat.Width)
        {
            roiHeight = Math.Clamp(roiHeight, 0, Capture.RawMat.Height);
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.Cam.RoiWindowH),
               roiHeight.ToString());
            BabbleCore.Instance.Settings.Save();
        }

        Mat sourceMat = Capture.RawMat, resultMat = new Mat(sourceMat, (roiX == 0 || roiY == 0 || roiWidth == 0 || roiHeight == 0 ||
            roiWidth == sourceMat.Width || roiHeight == sourceMat.Height) ? new Rect(0, 0, sourceMat.Width, sourceMat.Height) : new Rect(roiX, roiY, roiWidth, roiHeight));
        if (resultMat.Channels() >= 2)
        {
            var newMat = new Mat();
            if (useRedChannel)
                Cv2.ExtractChannel(resultMat, newMat, 0);
            else
                Cv2.CvtColor(resultMat, newMat, ColorConversionCodes.BGR2GRAY);
            resultMat.Dispose();
            resultMat = newMat;
        }
        {
            var newMat = new Mat();
            if (rotationRadians != 0 || camSettings.GuiHorizontalFlip || camSettings.GuiVerticalFlip)
            {
                double cos = Math.Cos(rotationRadians), sin = Math.Sin(rotationRadians);
                double scale = 1.0 / (Math.Abs(cos) + Math.Abs(sin));
                double hscale = (camSettings.GuiHorizontalFlip ? -1.0 : 1.0) * scale;
                double vscale = (camSettings.GuiVerticalFlip ? -1.0 : 1.0) * scale;
                using var matrix = new Mat(2, 3, MatType.CV_64F);
                matrix.GetArray<double>(out var data);
                data[0] = (double)size.Width / (double)resultMat.Width * cos * hscale;
                data[1] = (double)size.Height / (double)resultMat.Height * sin * hscale;
                data[2] = ((double)size.Width - ((double)size.Width * cos + (double)size.Height * sin) * hscale) * 0.5;
                data[3] = -(double)size.Width / (double)resultMat.Width * sin * vscale;
                data[4] = (double)size.Height / (double)resultMat.Height * cos * vscale;
                data[5] = ((double)size.Height + ((double)size.Width * sin - (double)size.Height * cos) * vscale) * 0.5;
                Cv2.WarpAffine(resultMat, newMat, matrix, size);
            }
            else
            {
                try
                {
                    Cv2.Resize(resultMat, newMat, size);
                }
                catch (Exception e)
                {
                    return Mat.Zeros(0, 0);
                }
            }
            resultMat.Dispose();
            resultMat = newMat;
        }

        // Verify That the matrix is in continuous memory layout - xlinka
        if (!resultMat.IsContinuous()) {
            resultMat.Dispose();
            throw new InvalidOperationException("Image Matrix is not continuous in memory layout");
        }
        return resultMat;
    }

    /// <summary>
    /// Shuts down any and all current Capture sources
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual void Terminate()
    {
        if (Capture is null)
        {
            throw new InvalidOperationException();
        }

        Capture.StopCapture();
    }
}
