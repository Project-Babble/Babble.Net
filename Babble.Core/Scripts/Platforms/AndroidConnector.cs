namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Special class for iOS, Android and UWP platforms where EmguCV VideoCapture is not fully implemented
/// Support for MJPEG video streams only presently!
/// </summary>
public class AndroidConnector : PlatformConnector
{
    private static readonly HashSet<string> SerialConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "/dev/tty" };
    
    public AndroidConnector(string Url) : base(Url)
    {
    }
    
    public override void Initialize()
    {
        base.Initialize();
        
        if (SerialConnections.Any(prefix => Url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            Capture = new SerialCameraCapture(Url);
        }
        else
        {
            // Default to IPCameraCapture
            Capture = new IPCameraCapture(Url);
        }

        Capture.StartCapture();
    }
}
