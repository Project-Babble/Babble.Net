namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Defines custom camera stream behviour
/// </summary>
public abstract class Capture
{
    protected readonly byte[] EmptyFrame = Enumerable.Repeat<byte>(0, 256).ToArray();

    public const int BABBLE_FRAME_SIZE = 256;
    public abstract string Url { get; set; }
    public abstract byte[] Frame { get; }
    public abstract (int width, int height) Dimensions { get; }
    public abstract bool IsReady { get; set; }
    public abstract bool StartCapture();
    public abstract bool StopCapture();

    public Capture(string Url)
    {
        if (string.IsNullOrEmpty(Url))
            throw new ArgumentNullException("(Camera) Capture Url cannot be null or empty.");

        this.Url = Url;
        IsReady = false;
    }
}
