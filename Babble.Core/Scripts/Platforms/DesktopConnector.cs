 namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Base class for camera capture and frame processing
/// </summary>
public class DesktopConnector : PlatformConnector
{
    private static readonly HashSet<string> SerialConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "com" };

    private static readonly HashSet<string> IPConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "local" };

    private static readonly HashSet<string> ImageConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "bmp", "gif", "ico", "jpeg", "jpg", "png", "psd", "tiff" };

    public DesktopConnector(string Url) : base(Url)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        // Determine if this is an IP Camera, Serial Camera, or something else
        if (SerialConnections.Any(prefix => Url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            Capture = new SerialCamera(Url);
        }
        else if (ImageConnections.Any(prefix => Url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            Capture = new ImageCapture(Url);
        }
        else if (IPConnections.Any(prefix => Url.EndsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            Capture = new IPCameraCapture(Url);
        }
        else
        {
            // TODO: Fix the IP Camera thingy in this lmao
            Capture = new EmguCVCapture(Url); 
        }

        Capture.StartCapture();
    }
}
