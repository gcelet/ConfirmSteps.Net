namespace ConfirmSteps.Internal;

using ConfirmSteps.Steps;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Defines an internal builder for a scenario step.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public interface IStepBuilder<T>
    where T : class
{
    /// <summary>
    /// Builds the scenario step.
    /// </summary>
    /// <returns>The built step.</returns>
    IStep<T> Build();

    /// <summary>
    /// Registers services required by the step.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection with registered services.</returns>
    IServiceCollection RegisterServices(IServiceCollection services);
}
