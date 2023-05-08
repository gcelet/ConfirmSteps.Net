namespace ConfirmSteps;

using ConfirmSteps.Steps.Wait;

public static class WaitStepExtensions
{
    public static IStepBuilderAppender<T> WaitStep<T>(this IStepBuilderAppender<T> stepBuilderAppender,
        DelayRange delay)
        where T : class
    {
        stepBuilderAppender.Append(new WaitStepBuilder<T>(delay));

        return stepBuilderAppender;
    }

    public static IStepBuilderAppender<T> WaitStep<T>(this IStepBuilderAppender<T> stepBuilderAppender, long min,
        long max)
        where T : class
    {
        DelayRange delayRange = new(min, max);
        stepBuilderAppender.Append(new WaitStepBuilder<T>(delayRange));

        return stepBuilderAppender;
    }
}