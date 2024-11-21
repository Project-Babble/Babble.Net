﻿using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Core.Scripts.Decoders;

public class ImageCapture : Capture
{
    public override uint FrameCount { get; protected set; }

    public ImageCapture(string Url) : base(Url)
    {
        
    }

    public override Mat RawFrame
    {
        get
        {
            if (File.Exists(Url))
            {
                if (CvInvoke.HaveImageReader(Url))
                {
                    FrameCount++;
                    return CvInvoke.Imread(Url, ImreadModes.Color);
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
                    var mat = CvInvoke.Imread(Url, ImreadModes.Color);
                    return (mat.Width, mat.Height);
                }
            }

            throw new FileNotFoundException();
        }
    }

    public override bool IsReady { get; protected set; }
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
