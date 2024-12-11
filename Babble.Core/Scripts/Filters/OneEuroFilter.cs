namespace Babble.Core.Scripts;

public class OneEuroFilter(float minCutoff, float beta)
{
    protected bool firstTime = true;
    protected float minCutoff = minCutoff;
    protected float beta = beta;
    protected LowpassFilter xFilt = new LowpassFilter();
    protected LowpassFilter dxFilt = new LowpassFilter();
    protected float dcutoff = 1;

    public float MinCutoff
    {
        get { return minCutoff; }
        set { minCutoff = value; }
    }

    public float Beta
    {
        get { return beta; }
        set { beta = value; }
    }

    public float Filter(float x, float rate)
    {
        float dx = firstTime ? 0 : (x - xFilt.Last()) * rate;
        if (firstTime)
        {
            firstTime = false;
        }

        var edx = dxFilt.Filter(dx, Alpha(rate, dcutoff));
        var cutoff = minCutoff + beta * Math.Abs(edx);

        return xFilt.Filter(x, Alpha(rate, cutoff));
    }

    protected float Alpha(float rate, float cutoff)
    {
        var tau = 1.0f / ((float)Math.Tau * cutoff);
        var te = 1.0f / rate;
        return 1.0f / (1.0f + tau / te);
    }
}

public class LowpassFilter
{
    public LowpassFilter()
    {
        firstTime = true;
    }

    protected bool firstTime;
    protected float hatXPrev;

    public float Last()
    {
        return hatXPrev;
    }

    public float Filter(float x, float alpha)
    {
        float hatX = 0;
        if (firstTime)
        {
            firstTime = false;
            hatX = x;
        }
        else
            hatX = alpha * x + (1 - alpha) * hatXPrev;

        hatXPrev = hatX;

        return hatX;
    }
}
