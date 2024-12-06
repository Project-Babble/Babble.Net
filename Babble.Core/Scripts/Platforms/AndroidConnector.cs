namespace Babble.Core.Scripts.Captures;

/// <summary>
/// Special class for iOS, Android and UWP platforms where VideoCapture is not fully implemented
/// Support for MJPEG video streams only presently!
/// </summary>
public class AndroidConnector : PlatformConnector
{
    private static readonly HashSet<string> _IPConnectionsPrefixes
        = new(StringComparer.OrdinalIgnoreCase) { "http", };

    private static readonly HashSet<string> _IPConnectionsSuffixes
        = new(StringComparer.OrdinalIgnoreCase) { "local", "local/" };

    protected override Type DefaultCapture => typeof(IPCameraCapture);
    
    public AndroidConnector(string Url) : base(Url)
    {
        Captures = new()
        {
            { (_IPConnectionsPrefixes, false), typeof(IPCameraCapture) },
            { (_IPConnectionsSuffixes, true), typeof(IPCameraCapture) }
        };
    }
}