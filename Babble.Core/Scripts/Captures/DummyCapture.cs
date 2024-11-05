using Emgu.CV;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Wrapper class for EmguCV. We use this class when we know our camera isn't a:
/// 1) Serial Camera
/// 2) IP Camera capture
/// 3) Or we aren't on an unsupported mobile platform (iOS or Android. Tizen/WatchOS are ok though??)
/// </summary>
public class DummyCapture : Capture
{
    public override string Url { get; set; }

    public override Mat Frame { get => EmptyMat; }

    public override (int width, int height) Dimensions => DefaultFrameDimensions;

    public override bool IsReady { get; set; }

    public DummyCapture(string Url) : base(Url)
    {
    }

    public override bool StartCapture()
    {
        IsReady = true;
        return true;
    }

    public override bool StopCapture()
    {
        IsReady = false;
        return true;
    }
}
