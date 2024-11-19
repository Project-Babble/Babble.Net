namespace Babble.Core.Scripts.Filters;

internal class OneEuroFilter
{
    private static float _minCutoff;
    private static float _speedCoefficient;
    private static float _dCutoff;
    private float _lastValue;
    private float _lastDeltaValue;
    private double _lastTime;

    internal OneEuroFilter(float minCutoff = 1.0f, float speedCoefficient = 0.0f, float dCutoff = 1.0f)
    {
        _minCutoff = minCutoff;
        _speedCoefficient = speedCoefficient;
        _dCutoff = dCutoff;
        _lastValue = 0;
        _lastDeltaValue = 0;
        _lastTime = -1;
    }

    private float Alpha(float cutoff, float deltaTime)
    {
        float tau = 1.0f / (MathF.Tau * cutoff);
        return 1.0f / (1.0f + tau / deltaTime);
    }

    internal float Filter(float value, double timestamp)
    {
        if (_lastTime == -1)
        {
            _lastValue = value;
            _lastTime = timestamp;
            return value;
        }

        float deltaTime = (float)(timestamp - _lastTime);

        // Compute velocity
        float deltaValue = (value - _lastValue) / deltaTime;
        float smoothedDelta = _lastDeltaValue;

        if (deltaTime > 0)
        {
            smoothedDelta = ExponentialSmoothing(
                deltaValue,
                _lastDeltaValue,
                Alpha(_dCutoff, deltaTime));
        }

        float cutoff = _minCutoff + _speedCoefficient * MathF.Abs(smoothedDelta);
        float smoothedValue = ExponentialSmoothing(
            value,
            _lastValue,
            Alpha(cutoff, deltaTime));

        // Update state
        _lastValue = smoothedValue;
        _lastDeltaValue = smoothedDelta;
        _lastTime = timestamp;

        return smoothedValue;
    }

    private float ExponentialSmoothing(float value, float lastValue, float alpha)
    {
        return alpha * value + (1 - alpha) * lastValue;
    }
}
