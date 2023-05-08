namespace ConfirmSteps.Steps.Wait;

using Microsoft.Extensions.DependencyInjection;

public sealed class WaitStep<T> : Step<T>
    where T : class
{
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