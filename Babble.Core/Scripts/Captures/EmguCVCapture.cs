using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Babble.Core.Scripts.Decoders
{
    /// <summary>
    /// Wrapper class for EmguCV. We use this class when we know our camera isn't a:
    /// 1) Serial Camera
    /// 2) IP Camera capture
    /// 3) Or we aren't on an unsupported mobile platform (iOS or Android. Tizen/WatchOS are ok though??)
    /// </summary>
    public class EmguCVCapture : Capture
    {
        /// <xlinka>
        /// Thread-safe lock object for synchronous access to video capture.
        /// </xlinka>
        private static readonly object lockObject = new object();

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
            get
            {
                lock (lockObject)
                {
                    if (_videoCapture is not null && _videoCapture.IsOpened)
                    {
                        try
                        {
                            // Attempt to retrieve frame within a 2-second timeout
                            var frameTask = Task.Run(() => _videoCapture.QueryFrame());
                            if (frameTask.Wait(TimeSpan.FromSeconds(2)) && frameTask.Result is not null)
                            {
                                return frameTask.Result;
                            }
                            else
                            {
                                /// <xlinka>
                                /// Logs a warning if retrieval failed or took too long and returns an empty frame.
                                /// </xlinka>
                                BabbleCore.Instance.Logger.LogWarning("Frame retrieval timed out or failed. Supplying empty frame.");
                                return EmptyMat;
                            }
                        }
                        catch (Exception ex)
                        {
                            /// <xlinka>
                            /// Logs an error for frame retrieval exceptions.
                            /// </xlinka>
                            BabbleCore.Instance.Logger.LogError($"Error retrieving frame: {ex.Message}");
                            return EmptyMat;
                        }
                    }

                    return EmptyMat;
                }
            }
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the dimensions of the video frame with timeout.
        /// </summary>
        /// <xlinka>
        /// Queries the dimensions (width, height) of the video feed frame within a 2-second timeout.
        /// </xlinka>
        public override (int width, int height) Dimensions
        {
            get
            {
                lock (lockObject)
                {
                    if (_videoCapture is not null && _videoCapture.IsOpened)
                    {
                        try
                        {
                            // Retrieve frame dimensions within 2-second timeout
                            var frameTask = Task.Run(() => _videoCapture.QueryFrame());
                            if (frameTask.Wait(TimeSpan.FromSeconds(2)) && frameTask.Result is not null)
                            {
                                var frame = frameTask.Result;
                                return (frame.Width, frame.Height);
                            }
                            else
                            {
                                /// <xlinka>
                                /// Logs a warning for dimension retrieval failure or timeout and returns default dimensions.
                                /// </xlinka>
                                BabbleCore.Instance.Logger.LogWarning("Dimension retrieval timed out or failed. Returning default dimensions.");
                                return DefaultFrameDimensions;
                            }
                        }
                        catch (Exception ex)
                        {
                            /// <xlinka>
                            /// Logs an error if an exception occurs during dimension retrieval.
                            /// </xlinka>
                            BabbleCore.Instance.Logger.LogError($"Error retrieving dimensions: {ex.Message}");
                            return DefaultFrameDimensions;
                        }
                    }

                    return DefaultFrameDimensions;
                }
            }
        }

        /// <summary>
        /// Indicates if the camera is ready for capturing frames.
        /// </summary>
        public override bool IsReady { get; set; }

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

            IsReady = _videoCapture.IsOpened;
            return IsReady;
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

            IsReady = false;
            _videoCapture.Dispose();
            return true;
        }
    }
}
