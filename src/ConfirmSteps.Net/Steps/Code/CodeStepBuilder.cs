namespace ConfirmSteps.Steps.Code;

using ConfirmSteps.Internal;
using Microsoft.Extensions.DependencyInjection;

public sealed class CodeStepBuilder<T> : IStepBuilder<T>
    where T : class
{
    private Func<StepContext<T>, CancellationToken, Task<ConfirmStatus>> ExecuteFunc { get; set; } =
        (_, _) => Task.FromResult(ConfirmStatus.Indecisive);

    public CodeStepBuilder<T> Execute(Func<StepContext<T>, CancellationToken, Task<ConfirmStatus>> execute)
    {
        ExecuteFunc = execute;

        return this;
    }

    public CodeStepBuilder<T> Execute(Func<StepContext<T>, ConfirmStatus> execute)
    {
        ExecuteFunc = (c, _) =>
        {
            ConfirmStatus confirmStatus = execute(c);
            return Task.FromResult(confirmStatus);
        };

        return this;
    }

    /// <inheritdoc />
    IStep<T> IStepBuilder<T>.Build()
    {
        return new CodeStep<T>(ExecuteFunc);
    }

    /// <inheritdoc />
    IServiceCollection IStepBuilder<T>.RegisterServices(IServiceCollection services)
    {
        return services;
    }
}