namespace Babble.Avalonia.Scripts;

public static class Float8Converter
{
    private const int TOTAL_BITS = 8;
    private const int EXPONENT_BITS = 4;
    private const int SIGNIFICAND_BITS = 3;

    private const int BIAS = 7; // 2^(4-1) - 1 for 4 exponent bits
    private const float EPSILON = 1.0f / 1024.0f; // Smallest representable positive number

    public static bool[] GetBits(float value)
    {
        bool[] bits = new bool[TOTAL_BITS];

        // Handle special cases first
        if (Math.Abs(value) < EPSILON)
        {
            return bits; // Return all zeros for values very close to zero
        }

        if (float.IsInfinity(value))
        {
            bits[7] = value < 0; // Sign bit
                                 // Set all exponent bits to 1
            for (int i = 3; i < 7; i++)
                bits[i] = true;
            // Set all significand bits to 0
            return bits;
        }

        if (float.IsNaN(value))
        {
            bits[7] = false; // Sign bit for NaN
                             // Set all exponent bits to 1
            for (int i = 3; i < 7; i++)
                bits[i] = true;
            // Set at least one significand bit to 1 for NaN
            bits[0] = true;
            return bits;
        }

        // Handle sign
        bits[7] = value < 0;
        value = Math.Abs(value);

        // Get exponent and significand
        int exponent = (int)Math.Floor(Math.Log2(value));
        float significand = value / (float)Math.Pow(2, exponent) - 1;

        // Normalize
        exponent += BIAS;

        // Handle denormalized numbers
        if (exponent <= 0)
        {
            exponent = 0;
            significand = value / (float)Math.Pow(2, -6); // -6 is the smallest normalized exponent
        }

        // Clamp exponent
        exponent = Math.Clamp(exponent, 0, 15); // 4 bits = max value of 15

        // Set exponent bits (bits 6-3)
        for (int i = 0; i < EXPONENT_BITS; i++)
        {
            bits[6 - i] = (exponent & (1 << i)) != 0;
        }

        // Set significand bits (bits 2-0)
        int significandInt = (int)(significand * 8); // 3 bits = 8 values
        for (int i = 0; i < SIGNIFICAND_BITS; i++)
        {
            bits[i] = (significandInt & (1 << i)) != 0;
        }

        return bits;
    }
}
