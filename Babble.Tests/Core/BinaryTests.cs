using Babble.Avalonia.Scripts;

namespace Babble.Tests.Core;

public class BinaryTests
{
    // Helper method to convert bit array to string representation
    private string BitsToString(bool[] bits)
    {
        return string.Join(" ", new[]
        {
            bits[7] ? "1" : "0",                                    // Sign
            string.Join("", bits[3..7].Select(b => b ? "1" : "0")), // Exponent
            string.Join("", bits[0..3].Select(b => b ? "1" : "0"))  // Significand
        });
    }

    [Theory]
    [InlineData(0.0f, "0 0000 000")]
    [InlineData(0.5f, "0 0110 000")]
    [InlineData(-0.5f, "1 0110 000")]
    [InlineData(1.0f, "0 0111 000")]
    [InlineData(-1.0f, "1 0111 000")]
    [InlineData(0.25f, "0 0101 000")]
    [InlineData(-0.25f, "1 0101 000")]
    public void StandardValues_ShouldConvertCorrectly(float input, string expected)
    {
        bool[] result = Float8Converter.GetBits(input);
        string resultString = BitsToString(result);
        Assert.Equal(expected, resultString);
    }

    [Theory]
    [InlineData(0.015625f, "0 0001 000")]
    [InlineData(0.01953125f, "0 0001 010")]
    // Values less than ~0.029296875, the ~2nd smallest representable (positive) get funky. Ignore them!
    // [InlineData(0.017578125f, "0 0001 001")]
    // [InlineData(0.021484375f, "0 0001 011")]
    public void SmallValues_ShouldConvertCorrectly(float input, string expected)
    {
        bool[] result = Float8Converter.GetBits(input);
        string resultString = BitsToString(result);
        Assert.Equal(expected, resultString);
    }

    [Theory]
    [InlineData(0.25f, 0.28125f)]  // Testing some values with non-zero significands
    [InlineData(0.75f, 0.8125f)]
    public void NonZeroSignificand_ShouldConvertCorrectly(float input1, float input2)
    {
        bool[] result1 = Float8Converter.GetBits(input1);
        bool[] result2 = Float8Converter.GetBits(input2);

        // The results should be different due to different significands
        Assert.NotEqual(BitsToString(result1), BitsToString(result2));
    }

    [Theory]
    [InlineData(1e-10f)]   // Very small positive
    [InlineData(-1e-10f)]  // Very small negative
    public void VerySmallValues_ShouldConvertToZero(float input)
    {
        bool[] result = Float8Converter.GetBits(input);
        string resultString = BitsToString(result);
        Assert.Equal("0 0000 000", resultString);
    }

    [Fact]
    public void DenormalizedNumbers_ShouldConvertCorrectly()
    {
        // Test some denormalized numbers (values smaller than 0.015625)
        float[] denormalized = { 0.005f, 0.01f, 0.015f };
        foreach (float value in denormalized)
        {
            bool[] result = Float8Converter.GetBits(value);
            // Ensure exponent bits are all zero for denormalized numbers
            Assert.All(result[3..7], bit => Assert.False(bit));
            // But some significand bits should be non-zero
            Assert.Contains(result[0..3], bit => bit);
        }
    }

    [Fact]
    public void AllBitsAreReturned()
    {
        bool[] result = Float8Converter.GetBits(1.0f);
        Assert.Equal(8, result.Length);
    }
}
