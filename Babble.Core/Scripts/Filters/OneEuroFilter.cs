namespace Babble.Core.Scripts;

public class OneEuroFilter
{
    public OneEuroFilter(float minCutoff, float beta)
    {
        firstTime = true;
        this.minCutoff = minCutoff;
        this.beta = beta;

        xFilt = new LowpassFilter();
        dxFilt = new LowpassFilter();
        dcutoff = 1;
    }

    protected bool firstTime;
    protected float minCutoff;
    protected float beta;
    protected LowpassFilter xFilt;
    protected LowpassFilter dxFilt;
    protected float dcutoff;

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
