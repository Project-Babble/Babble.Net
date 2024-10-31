namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Special class for iOS, Android and UWP platforms where EmguCV VideoCapture is not fully implemented
/// Support for MJPEG video streams only presently!
/// </summary>
public class MobileConnector : PlatformConnector
{
    public MobileConnector(string Url) : base(Url)
    {
    }

    /// <summary>
    /// Always use IPCameraCapture on mobile
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        Capture = new IPCameraCapture(Url);
        Capture.StartCapture();
        // WaitForCamera();
    }
}
