using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Scripts.EmguCV;

internal static class TensorUtils
{
    /// <summary>
    /// Represents the expected size of the input image,
    /// grayscale at 256x256px, 1 float per pixel, normalized to 0f-1f.
    /// </summary>
    private const int EXPECTED_SIZE = 256 * 256;

    /// <summary>
    /// Converts a float[256 * 256] array to a (dense) Tensor.
    /// An exception is raised if the array is the incorrect size.
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    internal static Tensor<float> PreprocessFrame(float[] frame)
    {
        if (frame.Length != EXPECTED_SIZE)
            throw new InvalidDataException();

        // var input = new DenseTensor<float>([1, 1, 256, 256]);
        // frame.AsSpan().CopyTo(input.Buffer.Span);

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
