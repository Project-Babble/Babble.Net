using Emgu.CV.CvEnum;
using Emgu.CV;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Maui.Scripts.Platforms;

internal class MobileConnector : IPlatformConnector
{
#pragma warning disable CS8618 
    private static MJPEGStreamDecoder _decoder;
#pragma warning restore CS8618

    public bool Initialize(string camera)
    {
        if (_decoder is not null)
        {
            throw new InvalidOperationException();
        }

        _decoder = new MJPEGStreamDecoder();
        _decoder.StartStream(camera);

        const int CONNECT_CAMERA_INTERVAL = 10;
        while (_decoder.Frame is null)
        {
            Thread.Sleep(CONNECT_CAMERA_INTERVAL);
        }

        return true;
    }

    public bool GetCameraData(out float[] data)
    {
        data = Array.Empty<float>();
        if (_decoder is null)
        {
            return false;
        }

        // Convert the frame to grayscale
        Mat grayFrame = new();
        CvInvoke.Imdecode(_decoder.Frame, ImreadModes.Grayscale, grayFrame);

        // Resize to the required 256 * 256
        Mat resizedFrame = new();
        CvInvoke.Resize(grayFrame, resizedFrame, new System.Drawing.Size(256, 256));

        data = Utils.ConvertMatToFloatArray(resizedFrame);

        return true;
    }

    public bool Terminate()
    {
        if (_decoder is not null)
        {
            throw new InvalidOperationException();
        }

        _decoder.Dispose();
        return true;
    }
}
