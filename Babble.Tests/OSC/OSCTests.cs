using Babble.OSC.Desktop.Mappings;
using Babble.OSC.Enums;

namespace Babble.OSC.Tests;

public class BabbleOSCTests
{
    [Fact]
    public void LoadExpressionMappings_ShouldGenerateValidMappings()
    {
        // Arrange
        var babbleOsc = new BabbleOSC("127.0.0.1", 8888);
        var type = typeof(BabbleOSC);
        var loadMappingsFromFileMethod = type.GetMethod("LoadMappingsFromFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        loadMappingsFromFileMethod.Invoke(babbleOsc, ["LipExpressionsManifest.txt"]);

        // Assert
        var mappings = (List<ExpressionMapping>)type.GetField("_mappings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                                                                  .GetValue(babbleOsc)!;
        Assert.NotEmpty(mappings);
        Assert.All(mappings, mapping =>
        {
            Assert.NotNull(mapping);
            Assert.NotNull(mapping.Address);
            Assert.NotNull(mapping.ComputeFloatValue);
        });
    }
}