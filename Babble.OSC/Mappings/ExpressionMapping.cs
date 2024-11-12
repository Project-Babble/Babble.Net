using Babble.Core.Enums;
using Babble.OSC.Collections;
using Babble.OSC.Enums;

namespace Babble.OSC.Desktop.Mappings;

public class ExpressionMapping
{
    public string Address { get; }
    public ExpressionPriority Priority { get; }
    public Func<Dictionary<UnifiedExpression, float>, float> ComputeFloatValue { get; }

    private int _frameCount = 0;
    private float _lastValue = 0f;
    private const float Threshold = 0.001f;

    public ExpressionMapping(
        string address,
        Func<Dictionary<UnifiedExpression, float>, float> computeValue,
        ExpressionPriority priority = ExpressionPriority.Medium)
    {
        Address = address;
        ComputeFloatValue = computeValue;
        Priority = priority;
    }

    public bool ShouldUpdate()
    {
        _frameCount++;
        if (_frameCount >= (int)Priority)
        {
            _frameCount = 0;
            return true;
        }
        return false;
    }

    public bool HasSignificantChange(float newValue)
    {
        if (Math.Abs(newValue - _lastValue) > Threshold)
        {
            _lastValue = newValue;
            return true;
        }
        return false;
    }
}
