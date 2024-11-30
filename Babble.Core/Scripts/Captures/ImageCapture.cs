using OpenCvSharp;

namespace Babble.Core.Scripts.Decoders;

public class ImageCapture : Capture
{
    public override uint FrameCount { get; protected set; }

    public ImageCapture(string Url) : base(Url)
    {
        
    }

    public override Mat RawMat
    {
        get
        {
            if (File.Exists(Url))
            {
                if (Cv2.HaveImageReader(Url))
                {
                    FrameCount++;
                    return Cv2.ImRead(Url, ImreadModes.Color);
                }
            }

            throw new FileNotFoundException();
        }
    }

    public override (int width, int height) Dimensions
    {
        get
        {
            if (File.Exists(Url))
            {
                if (Cv2.HaveImageReader(Url))
                {
                    var mat = Cv2.ImRead(Url, ImreadModes.Color);
                    return (mat.Width, mat.Height);
                }
            }

            throw new FileNotFoundException();
        }
    }

    public override bool IsReady { get; protected set; }
    public override string Url { get; set; } = null!;

    public override Task<bool> StartCapture()
    {
        IsReady = true;
        return Task.FromResult(true);
    }

    public override bool StopCapture()
    {
        IsReady = false;
        return true;
    }
}
