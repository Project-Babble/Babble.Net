using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Wrapper class for EmguCV. We use this class when we know our camera isn't a:
/// 1) Serial Camera
/// 2) IP Camera capture
/// 3) Or we aren't on an unsupported mobile platform (iOS or Android. Tizen/WatchOS are ok though??)
/// </summary>
public class EmguCVCapture : Capture
{
    /// <xlinka>
    /// VideoCapture instance to handle camera frames.
    /// </xlinka>
    private VideoCapture _videoCapture;

    /// <summary>
    /// Gets a raw frame from the camera with timeout for safety.
    /// </summary>
    /// <xlinka>
    /// Retrieves a raw frame from the camera feed within a 2-second timeout to prevent blocking.
    /// </xlinka>
    public override Mat RawFrame
    {
        get => _mat;
    }

    /// <summary>
    /// Retrieves the dimensions of the video frame with timeout.
    /// </summary>
    /// <xlinka>
    /// Queries the dimensions (width, height) of the video feed frame within a 2-second timeout.
    /// </xlinka>
    public override (int width, int height) Dimensions
    {
        get => _dimensions;
    }

    private Mat _mat = new Mat();
    private (int width, int height) _dimensions;

    /// <summary>
    /// Indicates if the camera is ready for capturing frames.
    /// </summary>
    public override bool IsReady { get; protected set; }

    /// <summary>
    /// Camera URL or source identifier.
    /// </summary>
    public override string Url { get; set; }

    /// <summary>
    /// Constructor that accepts a URL for the video source.
    /// </summary>
    /// <param name="Url">URL for video source.</param>
    public EmguCVCapture(string Url) : base(Url) { }

    /// <summary>
    /// Starts video capture and applies custom resolution and framerate settings.
    /// </summary>
    /// <returns>True if the video capture started successfully, otherwise false.</returns>
    /// <xlinka>
    /// Initializes the VideoCapture with the given URL or defaults to camera index 0 if unavailable.
    /// Applies custom resolution and framerate settings based on BabbleCore.
    /// </xlinka>
    public override bool StartCapture()
    {
        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
        {
            try
            {
                // Initialize VideoCapture with URL, timeout for robustness
                _videoCapture = Task.Run(() => new VideoCapture(Url), cts.Token).Result;
            }
            catch (AggregateException)
            {
                // Default to camera index 0 if URL-based capture fails
                const string defaultSource = "0";
                _videoCapture = new VideoCapture(defaultSource);
                BabbleCore.Instance.Settings.UpdateSetting<string>("capture_source", defaultSource);
                BabbleCore.Instance.Logger.LogWarning($"Failed to initialize VideoCapture with URL: {Url}. Defaulted to camera at index 0.");
            }
        }

        // Retrieve resolution and framerate settings from BabbleCore and apply
        var x = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_resolution_x");
        var y = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_resolution_y");
        var fr = BabbleCore.Instance.Settings.GetSetting<int>("gui_cam_framerate");

        if (x > 0) _videoCapture.Set(CapProp.FrameWidth, x);
        if (y > 0) _videoCapture.Set(CapProp.FrameHeight, y);
        if (fr > 0) _videoCapture.Set(CapProp.Fps, fr);

        _videoCapture.Start();
        _videoCapture.ImageGrabbed += VideoCapture_ImageGrabbed;

        IsReady = _videoCapture.IsOpened;
        return IsReady;
    }

    private void VideoCapture_ImageGrabbed(object? sender, EventArgs e)
    {
        try
        {
            if (_videoCapture.Retrieve(_mat))
            {
                _dimensions.width = _mat.Width;
                _dimensions.height = _mat.Height;
            }
        }
        catch (Exception)
        {

        }
    }

    /// <summary>
    /// Stops video capture and cleans up resources.
    /// </summary>
    /// <returns>True if capture stopped successfully, otherwise false.</returns>
    /// <xlinka>
    /// Disposes of the VideoCapture instance and sets IsReady to false to ensure resources are released.
    /// </xlinka>
    public override bool StopCapture()
    {
        if (_videoCapture is null)
            throw new InvalidOperationException("VideoCapture is not initialized.");

        _videoCapture.ImageGrabbed -= VideoCapture_ImageGrabbed;

        IsReady = false;
        _videoCapture.Stop();
        _videoCapture = null;
        return true;
    }
}