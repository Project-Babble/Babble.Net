namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Defines custom camera stream behviour
/// </summary>
public abstract class Capture
{
    public const int BABBLE_FRAME_SIZE = 256;
    public static readonly byte[] EmptyFrame = Array.Empty<byte>(); // BGR Color format
    public abstract string Url { get; set; }
    public abstract byte[] Frame { get; }
    public abstract (int width, int height) Dimensions { get; }
    public abstract bool IsReady { get; set; }
    public abstract bool StartCapture();
    public abstract bool StopCapture();

    public Capture(string Url)
    {
        this.Url = Url;
        IsReady = false;
    }
}
