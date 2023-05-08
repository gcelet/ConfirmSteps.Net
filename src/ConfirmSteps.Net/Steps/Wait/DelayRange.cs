namespace ConfirmSteps.Steps.Wait;

public sealed class DelayRange
{
    public DelayRange(double delay)
        : this(delay, delay)
    {
    }

    public DelayRange(double min, double max)
        : this(TimeSpan.FromMilliseconds(min), TimeSpan.FromMilliseconds(max))
    {
    }

    public DelayRange(TimeSpan delay)
        : this(delay, delay)
    {
    }

    public DelayRange(TimeSpan min, TimeSpan max)
    {
        Min = min;
        Max = max;
    }

    public static implicit operator DelayRange(long delay)
    {
        return new DelayRange(Convert.ToDouble(delay));
    }

    public static implicit operator DelayRange(TimeSpan delay)
    {
        return new DelayRange(delay);
    }

    public TimeSpan Max { get; }

    public TimeSpan Min { get; }

    public TimeSpan GetDelay(Random random)
    {
        double randomDouble = random.NextDouble();
        double lowerBound = Min.TotalMilliseconds;
        double upperBound = Max.TotalMilliseconds;
        double randomRangeDouble = randomDouble * (upperBound - lowerBound) + lowerBound;
        TimeSpan delay = TimeSpan.FromMilliseconds(randomRangeDouble);

        return delay;
    }
}