using static Babble.Avalonia.Scripts.Float8Converter;

public class BinaryParameterConverterTests
{
    [Theory]
    [InlineData(0f)]
    [InlineData(1f)]
    [InlineData(0.5f)]
    public void ConvertFloatToBinaryParameter_ValidPositiveRange_ReturnsExpectedResult(float value)
    {
        // Act
        var result = ConvertFloatToBinaryParameter(value);

        // Assert
        Assert.False(result.Negative);
        // Verify the converted value is within expected range when converted back
        var convertedBack = ConvertBinaryParameterToFloat(result);
        Assert.InRange(convertedBack, 0f, 1f);
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(-0.5f)]
    [InlineData(0f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    public void ConvertFloatToBinaryParameter_ValidNegativeRange_ReturnsExpectedResult(float value)
    {
        // Act
        var result = ConvertFloatToBinaryParameter(value, isCombined: true);

        // Assert
        Assert.Equal(value < 0, result.Negative);
        // Verify the converted value is within expected range when converted back
        var convertedBack = ConvertBinaryParameterToFloat(result);
        Assert.InRange(convertedBack, -1f, 1f);
    }

    [Theory]
    [InlineData(1.5f)]
    [InlineData(2f)]
    [InlineData(float.MaxValue)]
    public void ConvertFloatToBinaryParameter_AboveRange_ClampedToOne(float value)
    {
        // Act
        var result = ConvertFloatToBinaryParameter(value);

        // Assert
        Assert.False(result.Negative);
        var convertedBack = ConvertBinaryParameterToFloat(result);
        Assert.InRange(convertedBack, 0f, 1f);
    }

    [Theory]
    [InlineData(-1.5f)]
    [InlineData(-2f)]
    [InlineData(float.MinValue)]
    public void ConvertFloatToBinaryParameter_BelowRange_ClampedToNegativeOne(float value)
    {
        // Act
        var result = ConvertFloatToBinaryParameter(value, isCombined: true);

        // Assert
        Assert.True(result.Negative);
        var convertedBack = ConvertBinaryParameterToFloat(result);
        Assert.InRange(convertedBack, -1f, 1f);
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(-0.5f)]
    public void ConvertFloatToBinaryParameter_NegativeValueWithoutisCombined_ClampsToZero(float value)
    {
        // Act
        var result = ConvertFloatToBinaryParameter(value, isCombined: false);

        // Assert
        Assert.False(result.Negative);
        var convertedBack = ConvertBinaryParameterToFloat(result);
        Assert.InRange(convertedBack, 0f, 1f);
    }

    [Fact]
    public void ConvertFloatToBinaryParameter_NegativeOneValue_ReturnsAllTrue()
    {
        // Act
        var result = ConvertFloatToBinaryParameter(-1f, isCombined: true);

        // Assert
        Assert.True(result.Negative);
        Assert.True(result.Parameter1);
        Assert.True(result.Parameter2);
        Assert.True(result.Parameter4);
        Assert.True(result.Parameter8);
    }

    [Fact]
    public void ConvertFloatToBinaryParameter_ZeroValue_ReturnsAllFalse()
    {
        // Act
        var result = ConvertFloatToBinaryParameter(0f);

        // Assert
        Assert.False(result.Negative);
        Assert.False(result.Parameter1);
        Assert.False(result.Parameter2);
        Assert.False(result.Parameter4);
        Assert.False(result.Parameter8);
    }

    [Fact]
    public void ConvertFloatToBinaryParameter_OneValue_ReturnsAllTrue()
    {
        // Act
        var result = ConvertFloatToBinaryParameter(1f);

        // Assert
        Assert.False(result.Negative);
        Assert.True(result.Parameter1);
        Assert.True(result.Parameter2);
        Assert.True(result.Parameter4);
        Assert.True(result.Parameter8);
    }

    [Fact]
    public void ConvertBinaryParameterToFloat_AllFalse_ReturnsZero()
    {
        // Arrange
        var param = new BinaryParameterResult
        {
            Negative = false,
            Parameter1 = false,
            Parameter2 = false,
            Parameter4 = false,
            Parameter8 = false
        };

        // Act
        var result = ConvertBinaryParameterToFloat(param);

        // Assert
        Assert.Equal(0f, result);
    }

    [Fact]
    public void ConvertBinaryParameterToFloat_AllTrue_ReturnsOne()
    {
        // Arrange
        var param = new BinaryParameterResult
        {
            Negative = false,
            Parameter1 = true,
            Parameter2 = true,
            Parameter4 = true,
            Parameter8 = true
        };

        // Act
        var result = ConvertBinaryParameterToFloat(param);

        // Assert
        Assert.Equal(0.9375f, result); // 15/16 = 0.9375
    }
}