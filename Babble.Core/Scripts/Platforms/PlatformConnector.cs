using Babble.Core.Scripts.EmguCV;
using Emgu.CV;
using Emgu.CV.CvEnum;

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
    public float[] ExtractFrameData()
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
        if (Capture.RawMat.DataPointer == IntPtr.Zero) // Non-copying version of Capture.RawFrame.GetRawData().Length is null
        {
            return Array.Empty<float>();
        }
        if (Capture.FrameCount == _lastFrameCount)
        {
            return Array.Empty<float>();
        }

        _lastFrameCount = Capture.FrameCount;

        using Mat resultMat = TransformRawImage();

        using var finalMat = new Mat();
        resultMat.ConvertTo(finalMat, DepthType.Cv32F);

        // Convert to float array and normalize
        float[] floatArray = new float[finalMat.Rows * finalMat.Cols];
        finalMat.CopyTo(floatArray);

        // Normalize pixel values to [0, 1]
        for (int i = 0; i < floatArray.Length; i++)
        {
            floatArray[i] /= 255f;
        }

        return floatArray;
    }
    
    public Mat TransformRawImage()
    {
        // If this method is called from above, then the below checks don't apply
        // We need this in case we poll from Babble.Core.cs, in which the developer
        // Just wants the frame data, not expression data

        var emptyMat = Mat.Zeros(0, 0, DepthType.Cv8U, 1);
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
        if (Capture.RawMat.DataPointer == IntPtr.Zero) // Non-copying version of Capture.RawFrame.GetRawData().Length is null
        {
            return emptyMat;
        }

        var settings = BabbleCore.Instance.Settings;
        var camSettings = settings.Cam;
        var roiX = camSettings.RoiWindowX;
        var roiY = camSettings.RoiWindowY;
        var roiWidth = camSettings.RoiWindowW;
        var roiHeight = camSettings.RoiWindowH;
        var rotationAngle = camSettings.RotationAngle;
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

        // Process the image through our chain of operations
        using var processingChain = new MatProcessingChain();
        using Mat resultMat = processingChain
            .StartWith(Capture.RawMat, Capture.Dimensions)
            .UseRedChannel(useRedChannel)
            .Crop(roiX, roiY, roiWidth, roiHeight)
            .Rotate(rotationAngle)
            .Resize(new System.Drawing.Size(256, 256))
            .ApplyFlip(camSettings.GuiVerticalFlip, FlipType.Vertical)
            .ApplyFlip(camSettings.GuiHorizontalFlip, FlipType.Horizontal)
            .Result;

        // Verify That the matrix is in continuous memory layout - xlinka
        if (!resultMat.IsContinuous)
        {
            throw new InvalidOperationException("Image Matrix is not continuous in memory layout");
        }

        var clone = resultMat.Clone();
        return clone;
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
