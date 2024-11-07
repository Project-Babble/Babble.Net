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
    public override Mat RawFrame
    {
        get
        {
            if (_videoCapture is not null && _videoCapture.IsOpened)
            {
                // Call QueryFrame with a 100ms timeout
                var frameTask = Task.Run(() => _videoCapture.QueryFrame());
                if (frameTask.Wait(TimeSpan.FromMilliseconds(100)))
                {
                    var frame = frameTask.Result;
                    if (frame is not null)
                    {
                        return frame;
                    }
                }
            }

            return EmptyMat;
        }
        set => throw new NotImplementedException();
    }

    public override (int width, int height) Dimensions
    {
        get
        {
            if (_videoCapture is not null && _videoCapture.IsOpened)
            {
                // Call QueryFrame with a 100ms timeout
                var frameTask = Task.Run(() => _videoCapture.QueryFrame());
                if (frameTask.Wait(TimeSpan.FromMilliseconds(100)))
                {
                    var frame = frameTask.Result;
                    if (frame is not null)
                    {
                        return (frame.Width, frame.Height);
                    }
                }
            }

            return DefaultFrameDimensions;
        }
    }

    private (int width, int height) _dimensions;

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

        IsReady = _videoCapture.IsOpened;
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
