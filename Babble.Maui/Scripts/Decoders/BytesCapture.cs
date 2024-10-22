namespace Babble.Maui.Scripts.Decoders;

public abstract class BytesCapture : Capture
{
    protected BytesCapture(string Url) : base(Url)
    {
    }

    public abstract byte[] Frame { get; }
}
