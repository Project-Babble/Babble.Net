using Babble.Core;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Load the image using Emgu CV
            Mat frame = CvInvoke.Imread("0085.png");

            BabbleCore.StartInference();

            // Resize to the required 256 * 256
            Mat resizedFrame = new Mat();
            CvInvoke.Resize(frame, resizedFrame, new System.Drawing.Size(256, 256));

            // Convert the frame to grayscale
            Mat grayFrame = new Mat();
            CvInvoke.CvtColor(resizedFrame, grayFrame, ColorConversion.Bgr2Gray);

            // Convert the Mat to a float array
            float[] data = ConvertMatToFloatArray(grayFrame);

            if (!BabbleCore.GetExpressionData(data, out var exp))
                return;

            // Output the expression data
            foreach (var item in exp.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }

            BabbleCore.StopInference();
        }

        public static float[] ConvertMatToFloatArray(Mat mat)
        {
            // Ensure the Mat is of type CV_32F (float)
            if (mat.Depth != Emgu.CV.CvEnum.DepthType.Cv32F)
            {
                // Convert to CV_32F if needed
                mat.ConvertTo(mat, Emgu.CV.CvEnum.DepthType.Cv32F);
            }

            // Get the float array from the Mat
            float[] floatArray = new float[mat.Rows * mat.Cols];
            mat.CopyTo(floatArray);

            // Normalize pixel values to [0, 1] (assuming original values are 0-255)
            for (int i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] /= 255.0f;
            }

            return floatArray;
        }
    }
}
