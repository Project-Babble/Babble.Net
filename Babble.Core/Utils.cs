using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System.Reflection;

namespace Babble.Core;

internal static class Utils
{
    internal static Tensor<float> PreprocessFrame(Mat frame)
    {
        // Resize the frame to 256x256
        var resizedFrame = new Mat();
        Cv2.Resize(frame, resizedFrame, new Size(256, 256));

        // Convert the frame to grayscale
        var grayFrame = new Mat();
        Cv2.CvtColor(resizedFrame, grayFrame, ColorConversionCodes.BGR2GRAY);

        // Normalize the pixel values (1/255.0)
        grayFrame.ConvertTo(grayFrame, MatType.CV_32F, 1.0 / 255.0);

        // Prepare a tensor for ONNX input (1, 1, 256, 256)
        var input = new DenseTensor<float>([1, 1, 256, 256]);

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                input[0, 0, y, x] = grayFrame.At<float>(y, x);
            }
        }

        return input;
    }
}
