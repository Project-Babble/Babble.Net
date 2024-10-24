using Emgu.CV;
using Emgu.CV.CvEnum;

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
        // Fail fast
        if (Capture is null)
        {
            throw new InvalidOperationException();
        }

        if (Capture.Frame is null || Capture.Frame.Length == 0)
        {
            return Array.Empty<float>();
        }

        // Convert the frame to grayscale
        Mat grayFrame = new();
        CvInvoke.Imdecode(Capture.Frame, ImreadModes.Grayscale, grayFrame);

        // Resize to the required 256 * 256
        Mat resizedFrame = new();
        CvInvoke.Resize(grayFrame, resizedFrame, new System.Drawing.Size(256, 256));

        // Ensure the Mat is of type CV_32F (float)
        if (resizedFrame.Depth != DepthType.Cv32F)
        {
            // Convert to CV_32F if needed
            resizedFrame.ConvertTo(resizedFrame, DepthType.Cv32F);
        }

        // Get the float array from the Mat (for our ONNX model)
        float[] floatArray = new float[resizedFrame.Rows * resizedFrame.Cols];
        resizedFrame.CopyTo(floatArray);

        // Normalize pixel values to [0, 1] (assuming original values are 0-255)
        for (int i = 0; i < floatArray.Length; i++)
        {
            floatArray[i] /= 255f;
        }

        // Send it
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
        while (!Capture.IsReady)
        {
            Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
        }
    }
}
