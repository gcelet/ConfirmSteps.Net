namespace ConfirmSteps.Steps.Wait;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a step that waits for a specified duration before proceeding.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class WaitStep<T> : Step<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaitStep{T}"/> class with a specified delay range.
    /// </summary>
    /// <param name="delay">The delay range to wait for.</param>
    public WaitStep(DelayRange delay)
        : base(NullStepPreparer, new WaitStepExecutor(delay), NullStepVerifier, NullStepExtractor)
    {
    }

    private class WaitStepExecutor : IStepExecutor<T>
    {
        public WaitStepExecutor(DelayRange delay)
        {
            Delay = delay;
        }

        private DelayRange Delay { get; }

        /// <inheritdoc />
        public async Task<ConfirmStatus> ExecuteStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            Random random = stepContext.Services.GetRequiredService<Random>();
            TimeSpan delay = Delay.GetDelay(random);

            await Task.Delay(delay, cancellationToken);

            return ConfirmStatus.Success;
        }
    }
}
