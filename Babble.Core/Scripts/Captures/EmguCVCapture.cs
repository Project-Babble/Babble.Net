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
    private static readonly object lockObject = new object();

    public override Mat RawFrame
    {
        get
        {
            lock (lockObject)
            {
                if (_videoCapture is not null && _videoCapture.IsOpened)
                {
                    // Call QueryFrame with a 100ms timeout
                    var frame = _videoCapture.QueryFrame();
                    if (frame is not null)
                    {
                        return frame;
                    }

                    //var frameTask = Task.Run(() => _videoCapture.QueryFrame());
                    //if (frameTask.Wait(TimeSpan.FromMilliseconds(100)))
                    //{
                    //    var frame = frameTask.Result;
                    //    if (frame is not null)
                    //    {
                    //        return frame;
                    //    }
                    //}
                }

                return EmptyMat;
            }
        }
        set => throw new NotImplementedException();
    }

    public override (int width, int height) Dimensions
    {
        get
        {
            lock (lockObject)
            {
                if (_videoCapture is not null && _videoCapture.IsOpened)
                {
                    var frame = _videoCapture.QueryFrame();
                    if (frame is not null)
                    {
                        return (frame.Width, frame.Height);
                    }

                    //var frameTask = Task.Run(() => _videoCapture.QueryFrame());
                    //if (frameTask.Wait(TimeSpan.FromMilliseconds(100)))
                    //{
                    //    var frame = frameTask.Result;
                    //    if (frame is not null)
                    //    {
                    //        return (frame.Width, frame.Height);
                    //    }
                    //}
                }

                return DefaultFrameDimensions;
            }
        }
    }

    private (int width, int height) _dimensions;

    public override bool IsReady { get; set; }
    public override string Url { get; set; }

    private VideoCapture _videoCapture;

    public EmguCVCapture(string Url) : base(Url)
    {
    }

    public override bool StartCapture()
    {
        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
        {
            try
            {
                // Attempt to create VideoCapture with timeout
                _videoCapture = Task.Run(() =>
                {
                    return new VideoCapture(Url);
                }, cts.Token).Result;
            }
            catch (AggregateException)
            {
                // If timeout occurs or any other exception, fall back to default camera
                var url = "0";
                _videoCapture = new VideoCapture(url);
                BabbleCore.Instance.Settings.UpdateSetting<string>("capture_source", url);
            }
        }

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

        if (_videoCapture is null)
        {
            throw new InvalidOperationException();
        }

        return true;
    }
}
