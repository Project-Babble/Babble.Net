using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Core.Scripts.Decoders;

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
                EmptyFrame;
        }
    }

    public override (int width, int height) Dimensions
    {
        get
        {
            return !_videoCapture.IsOpened ?
                (_videoCapture.Width, _videoCapture.Height) :
                (BABBLE_FRAME_SIZE, BABBLE_FRAME_SIZE);
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

        var x = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_resolution_x");
        var y = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_resolution_y");
        var fr = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_framerate");

        if (x > 0)
        {
            _videoCapture.Set(CapProp.FrameWidth, x);
        }

        if (y > 0)
        {
            _videoCapture.Set(CapProp.FrameHeight, y);
        }

        if (fr > 0)
        {
            _videoCapture.Set(CapProp.Fps, fr);
        }

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
