namespace Babble.Core.Scripts.Captures;

/// <summary>
/// Special class for iOS, Android and UWP platforms where EmguCV VideoCapture is not fully implemented
/// Support for MJPEG video streams only presently!
/// </summary>
public class AndroidConnector : PlatformConnector
{
    private static readonly HashSet<string> _IPConnectionsPrefixes
        = new(StringComparer.OrdinalIgnoreCase) { "http", };

    private static readonly HashSet<string> _IPConnectionsSuffixes
        = new(StringComparer.OrdinalIgnoreCase) { "local", "local/" };
    
    public override Dictionary<(HashSet<string>, bool), Type> Captures { get; set; } = new()
    {
        { (_IPConnectionsPrefixes, false), typeof(IPCameraCapture) },
        { (_IPConnectionsSuffixes, true), typeof(IPCameraCapture) }
    };
    
    public override Type DefaultCapture => typeof(IPCameraCapture);
    
    public AndroidConnector(string Url) : base(Url)
    {
    }
}
