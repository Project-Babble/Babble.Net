namespace Babble.Core.Scripts.Config;

/// <summary>
/// Non-ObservableObject, implementation-agnostic calibraion item format
/// </summary>
public class CalibrationItem
{
    public required string ShapeName;
    public float Min;
    public float Max;
}
