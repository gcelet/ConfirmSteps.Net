namespace ConfirmSteps.Steps.Code;

/// <summary>
/// A scenario step that executes custom code.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class CodeStep<T> : Step<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeStep{T}"/> class.
    /// </summary>
    /// <param name="execute">The function to execute for the step.</param>
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
