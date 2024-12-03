using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Wrapper class for OpenCV. We use this class when we know our camera isn't a:
/// 1) Serial Camera
/// 2) IP Camera capture
/// 3) Or we aren't on an unsupported mobile platform (iOS or Android. Tizen/WatchOS are ok though??)
/// </summary>
public class OpenCVCapture : Capture
{
    /// <xlinka>
    /// VideoCapture instance to handle camera frames.
    /// </xlinka>
    private VideoCapture? _videoCapture;

    /// <summary>
    /// Gets a raw frame from the camera with timeout for safety.
    /// </summary>
    /// <xlinka>
    /// Retrieves a raw frame from the camera feed within a 2-second timeout to prevent blocking.
    /// </xlinka>
    public unsafe override Mat RawMat => _mat;

    private Mat _mat = new();

    public override uint FrameCount { get; protected set; }

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

    private (int width, int height) _dimensions;

    /// <summary>
    /// Indicates if the camera is ready for capturing frames.
    /// </summary>
    public override bool IsReady { get; protected set; }

    /// <summary>
    /// Camera URL or source identifier.
    /// </summary>
    public override string Url { get; set; } = null!;

    /// <summary>
    /// Constructor that accepts a URL for the video source.
    /// </summary>
    /// <param name="Url">URL for video source.</param>
    public OpenCVCapture(string Url) : base(Url) { }

    private bool _loop = false;

    /// <summary>
    /// Starts video capture and applies custom resolution and framerate settings.
    /// </summary>
    /// <returns>True if the video capture started successfully, otherwise false.</returns>
    /// <xlinka>
    /// Initializes the VideoCapture with the given URL or defaults to camera index 0 if unavailable.
    /// Applies custom resolution and framerate settings based on BabbleCore.
    /// </xlinka>
    public override async Task<bool> StartCapture()
    {
        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
        {
            try
            {
                // Initialize VideoCapture with URL, timeout for robustness
                if (!int.TryParse(Url, out var index))
                    _videoCapture = await Task.Run(() => VideoCapture.FromCamera(index), cts.Token);
                else
                    _videoCapture = await Task.Run(() => new VideoCapture(Url), cts.Token);
                _loop = true;
                Task.Run(VideoCapture_UpdateLoop);
            }
            catch (AggregateException)
            {
                // Default to camera index 0 if URL-based capture fails
                const string defaultSource = "0";
                _videoCapture = new VideoCapture(defaultSource);
                BabbleCore.Instance.Settings.UpdateSetting<string>(
                    nameof(BabbleCore.Instance.Settings.Cam.CaptureSource), 
                    defaultSource);
                BabbleCore.Instance.Logger.LogWarning($"Failed to initialize VideoCapture with URL: {Url}. Defaulted to camera at index 0.");
            }
        }

        IsReady = _videoCapture.IsOpened();
        return IsReady;
    }

    private Task VideoCapture_UpdateLoop()
    {
        while (_loop)
        {
            try
            {
                IsReady = _videoCapture?.Read(_mat) == true;
                if (IsReady)
                {
                    FrameCount++;
                    _dimensions.width = _mat.Width;
                    _dimensions.height = _mat.Height;
                }
            }
            catch (Exception) { }
        }

        return Task.CompletedTask;
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
            return false;

        _loop = false;
        IsReady = false;
        _videoCapture.Release();
        _videoCapture.Dispose();
        _videoCapture = null;
        return true;
    }
}
