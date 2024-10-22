using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Maui.Scripts.Decoders;

/// <summary>
/// Wrapper class for EmguCV for edge cases, IE Serial/IP Camera capture
/// </summary>
internal class EmguCVCapture : MatCapture
{
    public override Mat Frame
    {
        get
        {
            return !_videoCapture.IsOpened ? 
                _videoCapture.QueryFrame() : 
                Mat.Zeros(256, 256, DepthType.Cv32F, 1);
        }

    }

    public override bool IsReady { get; set; }

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
