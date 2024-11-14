using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Avalonia.OSC;

internal static class Float8Converter
{
    // Powers of 2 for binary parameters (1,2,4,8)
    internal static readonly int[] BinaryPowers = { 1, 2, 4, 8 };

    private const float MinFloat8 = -1f;
    private const float MaxFloat8 = 1f;
    private const int BitsFloat8 = 8;

    internal static bool[] GetBits(float value)
    {
        float normalized = ConvertTo8Bit(value);
        int intValue = (int)(normalized * ((1 << BitsFloat8) - 1));

        bool[] bits = new bool[BitsFloat8];
        for (int i = 0; i < BitsFloat8; i++)
        {
            bits[i] = (intValue & (1 << i)) != 0;
        }
        return bits;
    }

    private static float ConvertTo8Bit(float value)
    {
        // Clamp to 0-1 range
        value = Math.Clamp(value, MinFloat8, MaxFloat8);

        // Convert to 8-bit resolution
        int intValue = (int)(value * ((1 << BitsFloat8) - 1));

        return intValue / (float)((1 << BitsFloat8) - 1);
    }
}
