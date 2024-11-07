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
    public Capture Capture { get; set; }

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
        // Fail *fast*
        if (Capture is null)
        {
            return Array.Empty<float>();
        }
        if (!Capture.IsReady)
        {
            return Array.Empty<float>();
        }
        if (Capture.RawFrame is null)
        {
            return Array.Empty<float>();
        }
        if (Capture.RawFrame.GetRawData() is null)
        {
            return Array.Empty<float>();
        }

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
        if (Capture.RawFrame is null)
        {
            return emptyMat;
        }
        if (Capture.RawFrame.GetRawData() is null)
        {
            return emptyMat;
        }

        using var processingChain = new MatProcessingChain();
        var settings = BabbleCore.Instance.Settings;
        var roiX = settings.GetSetting<int>("roi_window_x");
        var roiY = settings.GetSetting<int>("roi_window_y");
        var roiWidth = settings.GetSetting<int>("roi_window_w");
        var roiHeight = settings.GetSetting<int>("roi_window_h");
        var rotationAngle = settings.GetSetting<double>("rotation_angle");
        var useRedChannel = settings.GetSetting<bool>("gui_use_red_channel");

        // Process the image through our chain of operations
        using Mat resultMat = processingChain
            .StartWith(Capture.RawFrame, Capture.Dimensions)
            .UseRedChannel(useRedChannel)
            .Rotate(rotationAngle)
            // .Crop(roiX, roiY, roiWidth, roiHeight)
            .Resize(new System.Drawing.Size(256, 256))
            .ApplyFlip("gui_vertical_flip", FlipType.Vertical)
            .ApplyFlip("gui_horizontal_flip", FlipType.Horizontal)
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
