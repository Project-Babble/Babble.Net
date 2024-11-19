using Babble.Core.Enums;

namespace Babble.Core.Scripts.Filters;

internal class ExpressionFilter
{
    private readonly Dictionary<UnifiedExpression, OneEuroFilter> _filters;

    internal ExpressionFilter(float minCutoff = 1.0f, float speedCoefficient = 0.0f, float dCutoff = 1.0f)
    {
        _filters = new Dictionary<UnifiedExpression, OneEuroFilter>();
        foreach (UnifiedExpression expression in Enum.GetValues<UnifiedExpression>())
        {
            _filters[expression] = new OneEuroFilter(minCutoff, speedCoefficient, dCutoff);
        }
    }

    internal float FilterExpression(UnifiedExpression expression, float value, double timestamp)
    {
        return _filters[expression].Filter(value, timestamp);
    }
}
