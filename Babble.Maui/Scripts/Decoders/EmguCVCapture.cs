using Emgu.CV;

namespace Babble.Maui.Scripts.Decoders;

/// <summary>
/// Wrapper class for EmguCV. We use this class when we know our camera isn't a:
/// 1) Serial Camera
/// 2) IP Camera capture
/// 3) Or we aren't on an unsupported mobile platform (iOS or Android. Tizen/WatchOS are ok though??)
/// </summary>
public class EmguCVCapture : Capture
{
    public override byte[] Frame
    {
        get
        {
            return !_videoCapture.IsOpened ?
                _videoCapture.QueryFrame().GetRawData() : 
                Array.Empty<byte>();
        }
    }

    public override bool IsReady { get; set; }
    public override string Url { get; set; }

    private VideoCapture _videoCapture;
    private Thread _thread;

    public EmguCVCapture(string Url) : base(Url)
    {
    }

    public override bool StartCapture()
    {
        _videoCapture = new VideoCapture(Url);

        //while (!_videoCapture.IsOpened)
        //{
        //    Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
        //}

        IsReady = true;
        return true;
    }

    public override bool StopCapture()
    {
        IsReady = false;
        if (_thread is not null)
        {
            _thread.Join();
        }

        if (_videoCapture is null)
        {
            throw new InvalidOperationException();
        }

        return true;
    }
}
