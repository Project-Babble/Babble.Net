using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Maui.Scripts;

internal static class Utils
{
    internal const int THREAD_TIMEOUT_MS = 10;

    internal static float[] ConvertMatToFloatArray(Mat mat)
    {
        // Ensure the Mat is of type CV_32F (float)
        if (mat.Depth != DepthType.Cv32F)
        {
            // Convert to CV_32F if needed
            mat.ConvertTo(mat, DepthType.Cv32F);
        }

        // Get the float array from the Mat
        float[] floatArray = new float[mat.Rows * mat.Cols];
        mat.CopyTo(floatArray);

        // Normalize pixel values to [0, 1] (assuming original values are 0-255)
        for (int i = 0; i < floatArray.Length; i++)
        {
            floatArray[i] /= 255f;
        }

        return floatArray;
    }

    internal static float[] ByteArrayToFloatArray(byte[] byteArray)
    {
        // Create a float array to store the float representation of the byte data
        float[] floatArray = new float[byteArray.Length];

        // Convert the byte data (0-255) to float (0.0-1.0)
        for (int i = 0; i < byteArray.Length; i++)
        {
            floatArray[i] = byteArray[i] / 255f;
        }

        return floatArray;
    }
}
