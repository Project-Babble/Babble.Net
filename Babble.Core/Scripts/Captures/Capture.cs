namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Defines custom camera stream behviour
/// </summary>
public abstract class Capture
{
    public static readonly (int width, int height) FrameDimensions = (240, 240);

    protected static readonly byte[] EmptyFrame = Array.Empty<byte>();

    public abstract string Url { get; set; }

    /// <summary>
    /// Represents the incoming frame data for this capture source. 
    /// Can be any dimension. This image must be 1 byte per pixel grayscale!
    /// </summary>
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
