using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Core.Scripts.Decoders;

public class ImageCapture : Capture
{
    public ImageCapture(string Url) : base(Url)
    {
        
    }

    public override byte[] Frame
    {
        get
        {
            if (File.Exists(Url))
            {
                if (CvInvoke.HaveImageReader(Url))
                {
                    return CvInvoke.Imread(Url, ImreadModes.Grayscale).GetRawData();
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
                if (CvInvoke.HaveImageReader(Url))
                {
                    var mat = CvInvoke.Imread(Url, ImreadModes.Grayscale);
                    return (mat.Width, mat.Height);
                }
            }

            throw new FileNotFoundException();
        }
    }

    public override bool IsReady { get; set; }
    public override string Url { get; set; }

    public override bool StartCapture()
    {
        IsReady = true;
        return true;
    }

    public override bool StopCapture()
    {
        IsReady = false;
        return true;
    }
}
