namespace ConfirmSteps.Steps.Wait;

/// <summary>
/// Represents a range of delay between a minimum and a maximum duration.
/// </summary>
public sealed class DelayRange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelayRange"/> class with a fixed delay.
    /// </summary>
    /// <param name="delay">The delay in milliseconds.</param>
    public DelayRange(double delay)
        : this(delay, delay)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayRange"/> class with a minimum and maximum delay.
    /// </summary>
    /// <param name="min">The minimum delay in milliseconds.</param>
    /// <param name="max">The maximum delay in milliseconds.</param>
    public DelayRange(double min, double max)
        : this(TimeSpan.FromMilliseconds(min), TimeSpan.FromMilliseconds(max))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayRange"/> class with a fixed delay.
    /// </summary>
    /// <param name="delay">The delay duration.</param>
    public DelayRange(TimeSpan delay)
        : this(delay, delay)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayRange"/> class with a minimum and maximum delay duration.
    /// </summary>
    /// <param name="min">The minimum delay duration.</param>
    /// <param name="max">The maximum delay duration.</param>
    public DelayRange(TimeSpan min, TimeSpan max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Implicitly converts a long value (milliseconds) to a <see cref="DelayRange"/>.
    /// </summary>
    /// <param name="delay">The delay in milliseconds.</param>
    public static implicit operator DelayRange(long delay)
    {
        return new DelayRange(Convert.ToDouble(delay));
    }

    /// <summary>
    /// Implicitly converts a <see cref="TimeSpan"/> to a <see cref="DelayRange"/>.
    /// </summary>
    /// <param name="delay">The delay duration.</param>
    public static implicit operator DelayRange(TimeSpan delay)
    {
        return new DelayRange(delay);
    }

    /// <summary>
    /// Gets the maximum delay duration.
    /// </summary>
    public TimeSpan Max { get; }

    /// <summary>
    /// Gets the minimum delay duration.
    /// </summary>
    public TimeSpan Min { get; }

    /// <summary>
    /// Returns a random delay within the range.
    /// </summary>
    /// <param name="random">The random number generator to use.</param>
    /// <returns>A random <see cref="TimeSpan"/> within the range.</returns>
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
