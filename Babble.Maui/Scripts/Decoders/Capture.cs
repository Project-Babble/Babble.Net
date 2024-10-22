namespace Babble.Maui.Scripts.Decoders;

public abstract class Capture
{
    public string Url { get; set; }
    public abstract bool IsReady { get; set; }
    public abstract bool StartCapture();
    public abstract bool StopCapture();

    public Capture(string Url)
    {
        this.Url = Url;
        IsReady = false;
    }
}
