namespace ConfirmSteps.Steps;

public abstract class Step<T> : IStep<T>
    where T : class
{
    protected Step(IStepPreparer<T> stepPreparer, IStepExecutor<T> stepExecutor,
        IStepVerifier<T> stepVerifier, IStepExtractor<T> stepExtractor)
    {
        StepPreparer = stepPreparer;
        StepExecutor = stepExecutor;
        StepVerifier = stepVerifier;
        StepExtractor = stepExtractor;
    }

    protected static IStepExtractor<T> NullStepExtractor { get; } = new NoOpStepExtractor();

    protected static IStepPreparer<T> NullStepPreparer { get; } = new NoOpStepPreparer();

    protected static IStepVerifier<T> NullStepVerifier { get; } = new NoOpStepVerifier();

    private IStepExecutor<T> StepExecutor { get; }

    private IStepExtractor<T> StepExtractor { get; }

    private IStepPreparer<T> StepPreparer { get; }

    private IStepVerifier<T> StepVerifier { get; }

    /// <inheritdoc />
    public async Task<StepResult<T>> ConfirmStep(StepContext<T> stepContext,
        CancellationToken cancellationToken)
    {
        StepProfiler stepProfiler = new();
        StepResult<T> stepResult = new()
        {
            State = StepState.Idle,
        };

        // Prepare
        using (stepProfiler.Profile("Prepare"))
        {
            stepResult.State = StepState.Prepare;
            try
            {
                stepResult.Status = await Prepare(stepContext, cancellationToken);
            }
            catch (Exception exception)
            {
                stepResult.Status = ConfirmStatus.Failure;
                stepResult.Exception = exception;
            }
        }

        // Execute
        if (stepResult.Status == ConfirmStatus.Success)
        {
            using (stepProfiler.Profile("Execute"))
            {
                stepResult.State = StepState.Execute;
                try
                {
                    stepResult.Status = await Execute(stepContext, cancellationToken);
                }
                catch (Exception exception)
                {
                    stepResult.Status = ConfirmStatus.Failure;
                    stepResult.Exception = exception;
                }
            }
        }

        // Verify
        if (stepResult.Status == ConfirmStatus.Success)
        {
            using (stepProfiler.Profile("Verify"))
            {
                stepResult.State = StepState.Verify;
                try
                {
                    stepResult.Status = await Verify(stepContext, cancellationToken);
                }
                catch (Exception exception)
                {
                    stepResult.Status = ConfirmStatus.Failure;
                    stepResult.Exception = exception;
                }
            }
        }

        // Extract
        if (stepResult.Status == ConfirmStatus.Success)
        {
            using (stepProfiler.Profile("Extract"))
            {
                try
                {
                    stepResult.State = StepState.Extract;
                    stepResult.Status = await Extract(stepContext, cancellationToken);
                }
                catch (Exception exception)
                {
                    stepResult.Status = ConfirmStatus.Failure;
                    stepResult.Exception = exception;
                }
            }
        }

        stepResult.State = StepState.Done;
        stepResult.Vars = stepContext.Vars;

        return stepResult;
    }

    protected virtual async Task<ConfirmStatus> Execute(StepContext<T> stepContext, CancellationToken cancellationToken)
    {
        ConfirmStatus status = await StepExecutor.ExecuteStep(stepContext, cancellationToken);

        return status;
    }

    protected virtual async Task<ConfirmStatus> Extract(StepContext<T> stepContext, CancellationToken cancellationToken)
    {
        ConfirmStatus status = await StepExtractor.ExtractStep(stepContext, cancellationToken);

        return status;
    }

    protected virtual async Task<ConfirmStatus> Prepare(StepContext<T> stepContext, CancellationToken cancellationToken)
    {
        ConfirmStatus status = await StepPreparer.PrepareStep(stepContext, cancellationToken);

        return status;
    }

    protected virtual async Task<ConfirmStatus> Verify(StepContext<T> stepContext, CancellationToken cancellationToken)
    {
        ConfirmStatus status = await StepVerifier.VerifyStep(stepContext, cancellationToken);

        return status;
    }

    private class NoOpStepExtractor : IStepExtractor<T>
    {
        /// <inheritdoc />
        public Task<ConfirmStatus> ExtractStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(ConfirmStatus.Success);
        }
    }

    private class NoOpStepPreparer : IStepPreparer<T>
    {
        /// <inheritdoc />
        public Task<ConfirmStatus> PrepareStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(ConfirmStatus.Success);
        }
    }

    private class NoOpStepVerifier : IStepVerifier<T>
    {
        /// <inheritdoc />
        public Task<ConfirmStatus> VerifyStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(ConfirmStatus.Success);
        }
    }
}