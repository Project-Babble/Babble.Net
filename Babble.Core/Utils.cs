using Microsoft.ML.OnnxRuntime.Tensors;

namespace Babble.Core;

internal static class Utils
{
    /// <summary>
    /// This function accepts a 256*256 float array. An exception is raised if this size does not match
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    internal static Tensor<float> PreprocessFrame(float[] frame)
    {
        var input = new DenseTensor<float>([1, 1, 256, 256]);

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                // Bit shift this 8 instead??
                input[0, 0, y, x] = frame[y * 256 + x];
            }
        }

        return input;
    }
}
