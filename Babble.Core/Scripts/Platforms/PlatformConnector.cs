using Babble.Core.Scripts.EmguCV;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Linq;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Manages what Captures are allowed to run on what platforms, as well as their Urls, etc.
/// </summary>
public abstract class PlatformConnector
{
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
    /// Converts our raw frame byte[] into something Babble can understand
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public float[] GetFrameData()
    {
        try
        {
            // Fail fast
            if (Capture is null)
            {
                return Array.Empty<float>();
            }
            if (!Capture.IsReady)
            {
                return Array.Empty<float>();
            }
            if (Capture.Frame is null)
            {
                return Array.Empty<float>();
            }
            if (Capture.Frame.Length == 0)
            {
                return Array.Empty<float>();
            }
        }
        catch
        {
            return Array.Empty<float>();
        }

        using var processingChain = new MatProcessingChain();

        // Get ROI settings
        var roiX = BabbleCore.Instance.Settings.GetSetting<int>("roi_window_x");
        var roiY = BabbleCore.Instance.Settings.GetSetting<int>("roi_window_y");
        var roiWidth = BabbleCore.Instance.Settings.GetSetting<int>("roi_window_w");
        var roiHeight = BabbleCore.Instance.Settings.GetSetting<int>("roi_window_h");
        var rotationAngle = BabbleCore.Instance.Settings.GetSetting<int>("rotation_angle");
        var useRedChannel = BabbleCore.Instance.Settings.GetSetting<bool>("gui_use_red_channel");

        // Process the image through our chain of operations
        using Mat finalMat = processingChain
            .StartWith(Capture.Frame, Capture.Dimensions)
            .Normalize(useRedChannel)
            .Rotate(rotationAngle)
            .Crop(roiX, roiY, roiWidth, roiHeight)
            .Resize(new System.Drawing.Size(256, 256))
            .EnsureDepth(DepthType.Cv32F)
            .ApplyFlip("gui_vertical_flip", FlipType.Vertical)
            .ApplyFlip("gui_horizontal_flip", FlipType.Horizontal)
            .Result;

        // Debugging
        // CvInvoke.Imwrite("output.png", finalMat);

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

    /// <summary>
    /// Shuts down any and all current Capture sources
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Terminate()
    {
        if (Capture is null)
        {
            throw new InvalidOperationException();
        }

        Capture.StopCapture();
    }

    /// <summary>
    /// Blocks this thread until Capture's Camera is ready. No timeout ATM
    /// </summary>
    public void WaitForCamera()
    {
        //while (!Capture.IsReady)
        //{
        //    Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
        //}
    }
}
