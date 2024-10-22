using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Maui.Scripts.Decoders;

public abstract class PlatformConnector
{
    public string Url { get; set; }

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
    /// Get the current frame data for our Babble model
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public float[] GetFrameData()
    {
        if (Capture is null)
        {
            throw new InvalidOperationException();
        }

        switch (Capture)
        {
            case MatCapture mc:
                return PreprocessFrameData(mc.Frame);
            case BytesCapture bc:
                return PreprocessFrameData(bc.Frame);
            default:
                throw new Exception("How did we get here");
        }
    }

    /// <summary>
    /// 
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
    /// Converts our raw byte array into something Babble can understand
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    protected float[] PreprocessFrameData(byte[] frame)
    {
        // Convert the frame to grayscale
        Mat grayFrame = new();
        CvInvoke.Imdecode(frame, ImreadModes.Grayscale, grayFrame);

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

    protected float[] PreprocessFrameData(Mat mat)
    {
        if (mat is null)
            return Array.Empty<float>();

        // Convert the frame to grayscale
        Mat grayFrame = new();
        CvInvoke.Imdecode(mat, ImreadModes.Grayscale, grayFrame);

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
