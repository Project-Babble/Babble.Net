using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Defines custom camera stream behavior
/// </summary>
public abstract class Capture
{
    /// <summary>
    /// Represents the size of a "default" Babble frame 
    /// Pulled from the Babble Board ESP32 cam
    /// </summary>
    public static readonly (int width, int height) DefaultFrameDimensions = (240, 240);

    /// <summary>
    /// Empty frame, used when data is bad and we need to return something
    /// </summary>
    protected static readonly Mat EmptyMat = Mat.Zeros(DefaultFrameDimensions.width, DefaultFrameDimensions.height, DepthType.Cv8U, 1);

    public abstract string Url { get; set; }

    /// <summary>
    /// Represents the incoming frame data for this capture source. 
    /// Can be any dimension, BGR color space
    /// </summary>
    public abstract Mat Frame { get; }

    /// <summary>
    /// Dimensions for this frame. Needs to be explicitly defined when dealing with 
    /// custom capture sources (IE Serial/Cameras)
    /// </summary>
    public abstract (int width, int height) Dimensions { get; }

    /// <summary>
    /// Is this Capture source ready to produce data?
    /// </summary>
    public abstract bool IsReady { get; set; }

    /// <summary>
    /// Start Capture on this source
    /// </summary>
    /// <returns></returns>
    public abstract bool StartCapture();

    /// <summary>
    /// Stop Capture on this source
    /// </summary>
    /// <returns></returns>
    public abstract bool StopCapture();

    public Capture(string Url)
    {
        this.Url = Url;
        IsReady = false;
    }
}
