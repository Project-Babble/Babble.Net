 namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Base class for camera capture and frame processing
/// </summary>
public class DesktopConnector : PlatformConnector
{
    private static readonly HashSet<string> SerialConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "com" };

    private static readonly HashSet<string> IPConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "http" };

    private static readonly HashSet<string> ImageConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "bmp", "gif", "ico", "jpeg", "jpg", "png", "psd", "tiff" };

    public DesktopConnector(string Url) : base(Url)
    {
    }

    // TODO Add loading camera from saved config
    public override void Initialize()
    {
        base.Initialize();

        // Determine if this is an IP Camera, Serial Camera, or something else
        if (SerialConnections.Any(prefix => Url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            Capture = new SerialCamera(Url);
        }
        else if (ImageConnections.Any(prefix => Url.EndsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            Capture = new ImageCapture(Url);
        }
        // On non-mobile platforms, we'll use EmguCVCapture for IP Cameras
        //else if (IPConnections.Any(prefix => Url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        //{
        //    Capture = new IPCameraCapture(Url);
        //}
        else
        {
            // Capture = new DummyCapture(Url);
            Capture = new EmguCVCapture(Url); 
        }

        Capture.StartCapture();
        WaitForCamera();
    }
}
