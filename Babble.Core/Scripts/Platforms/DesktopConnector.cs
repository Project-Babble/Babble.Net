 namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Base class for camera capture and frame processing
/// </summary>
public class DesktopConnector : PlatformConnector
{
    private static readonly HashSet<string> SerialConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "com" };

    private static readonly HashSet<string> IPConnectionsPrefixes
    = new(StringComparer.OrdinalIgnoreCase) { "http", };

    private static readonly HashSet<string> IPConnectionsSuffixes
        = new(StringComparer.OrdinalIgnoreCase) { "local", "local/" };

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
            Capture = new SerialCameraCapture(Url);
        }
        else if (IPConnectionsPrefixes.Any(prefix => Url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                 IPConnectionsSuffixes.Any(suffix => Url.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))))
        {
            Capture = new IPCameraCapture(Url);
        }
        else
        {
            // IPConnections on MacOS Fail here, so use the above implementation
            Capture = new OpenCVCapture(Url); 
        }

        Capture.StartCapture();
    }
}
