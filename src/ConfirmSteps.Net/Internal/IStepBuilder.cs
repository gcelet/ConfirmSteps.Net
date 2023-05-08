namespace ConfirmSteps.Internal;

using ConfirmSteps.Steps;
using Microsoft.Extensions.DependencyInjection;

public interface IStepBuilder<T>
    where T : class
{
    IStep<T> Build();

    IServiceCollection RegisterServices(IServiceCollection services);
}