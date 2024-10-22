using Emgu.CV;

namespace Babble.Maui.Scripts.Decoders;

public abstract class MatCapture : Capture
{
    protected MatCapture(string Url) : base(Url)
    {
    }

    public abstract Mat Frame { get; }
}
