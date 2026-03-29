namespace ConfirmSteps;

using ConfirmSteps.Data;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides methods to customize a scenario before building it.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IScenarioCustomizer<T>
    where T : class
{
    /// <summary>
    /// Configures global variables for the scenario.
    /// </summary>
    /// <param name="globalBuilder">An action to configure global variables using <see cref="VarBuilder{T}"/>.</param>
    /// <returns>The current <see cref="IScenarioCustomizer{T}"/> instance for fluent chaining.</returns>
    IScenarioCustomizer<T> WithGlobals(Action<VarBuilder<T>> globalBuilder);

    /// <summary>
    /// Configures dependency injection services for the scenario.
    /// </summary>
    /// <param name="services">An action to configure the <see cref="IServiceCollection"/>.</param>
    /// <returns>The current <see cref="IScenarioCustomizer{T}"/> instance for fluent chaining.</returns>
    IScenarioCustomizer<T> WithServices(Action<IServiceCollection> services);

    /// <summary>
    /// Configures the steps for the scenario.
    /// </summary>
    /// <param name="stepBuilderAppender">An action to add steps to the scenario using <see cref="IStepBuilderAppender{T}"/>.</param>
    /// <returns>An <see cref="IScenarioBuilder{T}"/> instance to finish building the scenario.</returns>
    IScenarioBuilder<T> WithSteps(Action<IStepBuilderAppender<T>> stepBuilderAppender);
}
