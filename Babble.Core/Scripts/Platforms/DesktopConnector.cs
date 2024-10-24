namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Base class for camera capture and frame processing
/// </summary>
public class DesktopConnector : PlatformConnector
{
    public DesktopConnector(string Url) : base(Url)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        // TODO Add loading camera from saved config
        // TODO Add logic to reload camera on change

        // Determine if this is an IP Camera, Serial Camera, or something else
        // Base Capture class reuses logic to check for empty/null strings
        var str = Url.ToLower();
        if (str.StartsWith("com"))
        {
            Capture = new SerialCamera(Url);
        }
        else if (str.StartsWith("http"))
        {
            Capture = new IPCameraCapture(Url);
        }
        else
        {
            // Capture = new EmguCVCapture(Url);
            Capture = new DummyCapture(Url);
        }

        Capture.StartCapture();
        WaitForCamera();
    }
}
