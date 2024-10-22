namespace Babble.Maui.Scripts.Decoders;

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
        var str = Url.ToLower();
        if (str.StartsWith("com"))
        {
            Capture = new SerialCameraCapture(Url);
        }
        else if (str.StartsWith("http"))
        {
            Capture = new IPCameraCapture(Url);
        }
        else
        {
            Capture = new EmguCVCapture(Url);
        }

        Capture.StartCapture();
        WaitForCamera();
    }
}
