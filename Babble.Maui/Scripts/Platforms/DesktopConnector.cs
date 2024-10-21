using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Maui.Scripts.Platforms;

internal class DesktopConnector : IPlatformConnector
{
    private static VideoCapture _capture;

    public bool Initialize(string camera)
    {
        if (_capture is not null)
        {
            throw new InvalidOperationException();
        }

        // TODO Add loading camera from saved config
        // TODO Add logic to reload camera on change
        try
        {
            // Initialize the capture from the IP camera
            _capture = new VideoCapture(camera);

            while (!_capture.IsOpened)
            {
                Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
            }
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }

    public bool GetCameraData(out float[] data)
    {
        data = Array.Empty<float>();
        if (_capture is null)
        {
            return false;
        }

        using Mat frame = _capture.QueryFrame();
        if (frame is null)
            return false;

        // Resize to the required 256 * 256
        Mat resizedFrame = new();
        CvInvoke.Resize(frame, resizedFrame, new System.Drawing.Size(256, 256));

        // Convert the frame to grayscale
        Mat grayFrame = new();
        CvInvoke.CvtColor(resizedFrame, grayFrame, ColorConversion.Bgr2Gray);
        data = Utils.ConvertMatToFloatArray(grayFrame);

        return true;
    }

    public bool Terminate()
    {
        if (_capture is null)
        {
            throw new InvalidOperationException();
        }

        return true;
    }
}
