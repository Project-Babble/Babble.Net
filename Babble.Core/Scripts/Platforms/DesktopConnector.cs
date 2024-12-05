namespace Babble.Core.Scripts.Captures;

/// <summary>
/// Base class for camera capture and frame processing
/// </summary>
public class DesktopConnector : PlatformConnector
{
    private static readonly HashSet<string> _serialConnections 
        = new(StringComparer.OrdinalIgnoreCase) { "com" };

    private static readonly HashSet<string> _IPConnectionsPrefixes
        = new(StringComparer.OrdinalIgnoreCase) { "http", };

    private static readonly HashSet<string> _IPConnectionsSuffixes
        = new(StringComparer.OrdinalIgnoreCase) { "local", "local/" };
    
    public override Type DefaultCapture => typeof(OpenCVCapture);

    public DesktopConnector(string Url) : base(Url)
    {
        Captures = new()
        {
            { (_serialConnections, false), typeof(SerialCameraCapture) },
            { (_IPConnectionsPrefixes, false), typeof(IPCameraCapture) },
            { (_IPConnectionsSuffixes, true), typeof(IPCameraCapture) }
        };
    }
}