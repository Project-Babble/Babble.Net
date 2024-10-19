

using Babble.Core;
using OpenCvSharp;

namespace Babble.CLI;

internal class Program
{
    static void Main(string[] args)
    {
        //VideoCapture capture = new(0); // 0 = default camera
        //if (!capture.IsOpened())
        //{
        //    Console.WriteLine("Uh oh.");
        //    return;
        //}

        var frame = Cv2.ImRead("0085.png");

        BabbleCore.StartInference();

        // using var frame = new Mat();
        // capture.Read(frame);

        //if (frame.Empty())
        //    continue;

        // Resize to the required 256 * 256
        var resizedFrame = new Mat();
        Cv2.Resize(frame, resizedFrame, new Size(256, 256));

        // Convert the frame to grayscale
        var grayFrame = new Mat();
        Cv2.CvtColor(resizedFrame, grayFrame, ColorConversionCodes.BGR2GRAY);
        var data = ConvertMatToFloatArray(grayFrame);

        if (!BabbleCore.GetExpressionData(data, out var exp))
            return;

        foreach (var item in exp.OrderByDescending(x => x.Value))
            Console.WriteLine($"{item.Key}: {item.Value}");

        BabbleCore.StopInference();
    }

    public static float[] ConvertMatToFloatArray(Mat mat)
    {
        // Ensure that the Mat is of type CV_32F (float)
        if (mat.Type() != MatType.CV_32F)
        {
            // Convert to CV_32F if needed
            mat.ConvertTo(mat, MatType.CV_32F);
        }

        mat.GetArray(out float[] floatArray);

        for (int i = 0; i < floatArray.Length; i++)
        {
            // Normalize pixel values to [0, 1]
            // Assuming pixel values were originally 0-255
            floatArray[i] /= 255.0f; 
        }

        return floatArray;
    }
}
