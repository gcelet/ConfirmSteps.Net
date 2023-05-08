namespace ConfirmSteps.Steps.Code;

public sealed class CodeStep<T> : Step<T>
    where T : class
{
    public CodeStep(Func<StepContext<T>, CancellationToken, Task<ConfirmStatus>> execute)
        : base(NullStepPreparer, new CodeStepExecutor(execute), NullStepVerifier, NullStepExtractor)
    {
    }

    private class CodeStepExecutor : IStepExecutor<T>
    {
        public CodeStepExecutor(Func<StepContext<T>, CancellationToken, Task<ConfirmStatus>> execute)
        {
            Execute = execute;
        }

        private Func<StepContext<T>, CancellationToken, Task<ConfirmStatus>> Execute { get; }

        /// <inheritdoc />
        public Task<ConfirmStatus> ExecuteStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            return Execute.Invoke(stepContext, cancellationToken);
        }
    }
}