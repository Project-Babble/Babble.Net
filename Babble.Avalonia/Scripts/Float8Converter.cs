namespace Babble.Avalonia.Scripts;

public static class Float8Converter
{
    public static BinaryParameterResult ConvertFloatToBinaryParameter(float value, bool isCombined = false)
    {
        // Validate and clamp input range
        if (isCombined)
        {
            value = Math.Clamp(value, -1f, 1f);
        }
        else
        {
            value = Math.Clamp(value, 0f, 1f);
        }

        var result = new BinaryParameterResult();

        // Handle negative values
        if (isCombined)
        {
            result.Negative = value < 0;
            value = Math.Abs(value);
        }

        // Convert the float to a normalized value between 0 and 31 (for 5 bits)
        int normalizedValue = (int)(value * 31);

        // Extract individual parameters using bitwise operations
        result.Parameter8 = (normalizedValue & 16) != 0;  // 2^4 (16)
        result.Parameter4 = (normalizedValue & 8) != 0;   // 2^3 (8)
        result.Parameter2 = (normalizedValue & 4) != 0;   // 2^2 (4)
        result.Parameter1 = (normalizedValue & 2) != 0;   // 2^1 (2)

        return result;
    }

    // Helper method to convert binary parameters back to float for verification
    public static float ConvertBinaryParameterToFloat(BinaryParameterResult param)
    {
        float value = 0f;

        // Add up the values based on the binary parameters
        if (param.Parameter1) value += 1f / 16f;  // 2^(-4)
        if (param.Parameter2) value += 1f / 8f;   // 2^(-3)
        if (param.Parameter4) value += 1f / 4f;   // 2^(-2)
        if (param.Parameter8) value += 1f / 2f;   // 2^(-1)

        // Apply sign if negative
        if (param.Negative) value = -value;

        return value;
    }

    public class BinaryParameterResult
    {
        public bool Negative { get; set; }
        public bool Parameter1 { get; set; }
        public bool Parameter2 { get; set; }
        public bool Parameter4 { get; set; }
        public bool Parameter8 { get; set; }
    }
}
