namespace Babble.Maui.Scripts.Decoders;

/// <summary>
/// Special class for iOS, Android and UWP platforms where EmguCV VideoCapture is not fully implemented
/// Support for MJPEG video streams only presently!
/// </summary>
internal class MobileConnector : PlatformConnector
{
    public MobileConnector(string Url) : base(Url)
    {
    }

    public override void Initialize()
    {
        // Always use IPCameraCapture on mobile
        base.Initialize();
        Capture = new IPCameraCapture(Url);
        Capture.StartCapture();
        WaitForCamera();
    }
}
