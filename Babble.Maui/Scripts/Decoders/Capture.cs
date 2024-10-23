namespace Babble.Maui.Scripts.Decoders;

/// <summary>
/// Defines custom camera stream behviour
/// </summary>
public abstract class Capture
{
    public abstract string Url { get; set; }
    public abstract byte[] Frame { get; }
    public abstract bool IsReady { get; set; }
    public abstract bool StartCapture();
    public abstract bool StopCapture();

    public Capture(string Url)
    {
        this.Url = Url;
        IsReady = false;
    }
}
