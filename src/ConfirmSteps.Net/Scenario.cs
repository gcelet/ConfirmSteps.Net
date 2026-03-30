namespace ConfirmSteps;

using ConfirmSteps.Data;
using ConfirmSteps.Steps;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides a static entry point to create new scenarios.
/// </summary>
public static class Scenario
{
    /// <summary>
    /// Starts the creation of a new scenario.
    /// </summary>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="title">The title of the scenario.</param>
    /// <returns>An <see cref="IScenarioCustomizer{T}"/> to configure the scenario.</returns>
    public static IScenarioCustomizer<T> New<T>(string title)
        where T : class
    {
        return new ScenarioBuilder<T>(title);
    }
}

/// <summary>
/// Represents a scenario consisting of multiple steps to be executed.
/// </summary>
/// <typeparam name="T">The type of the data object being processed.</typeparam>
public sealed class Scenario<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Scenario{T}"/> class.
    /// </summary>
    /// <param name="title">The title of the scenario.</param>
    /// <param name="steps">The list of steps to execute.</param>
    /// <param name="services">The service provider for dependency injection.</param>
    public Scenario(string title, IReadOnlyList<IStep<T>> steps, IServiceProvider services)
    {
        Title = title;
        Steps = steps;
        Services = services;
    }

    /// <summary>
    /// Gets the title of the scenario.
    /// </summary>
    public string Title { get; }

    private IServiceProvider Services { get; }

    private IReadOnlyList<IStep<T>> Steps { get; }

    /// <summary>
    /// Executes the scenario with the provided data.
    /// </summary>
    /// <param name="data">The initial data object.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>A <see cref="ConfirmStepResult{T}"/> containing the results of the execution.</returns>
    public async Task<ConfirmStepResult<T>> ConfirmSteps(T data, CancellationToken cancellationToken)
    {
        int nbSteps = Steps.Count;
        VarManager<T> varManager = Services.GetRequiredService<VarManager<T>>();
        IReadOnlyDictionary<string, object> globalVars = varManager.Extract(data);
        IServiceScopeFactory serviceScopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
        ScenarioContext<T> scenarioContext = new(data, Services)
        {
            Vars = new Dictionary<string, object>(globalVars, StringComparer.Ordinal)
        };
        Exception? scenarioException = null;
        List<StepResult<T>> stepResults = new();
        ConfirmStatus scenarioStatus = ConfirmStatus.Success;

        for (int i = 0; i < nbSteps; i++)
        {
            StepResult<T> stepResult;
            if (scenarioStatus == ConfirmStatus.Success)
            {
                using IServiceScope scope = serviceScopeFactory.CreateScope();
                IServiceProvider serviceProvider = scope.ServiceProvider;
                StepContext<T> stepContext = new(scenarioContext, serviceProvider, scenarioContext.Vars);
                IStep<T> currentStep = Steps[i];

                stepResult =
                    await currentStep.ConfirmStep(stepContext, cancellationToken).ConfigureAwait(false);
                scenarioContext.Vars = stepResult.Vars.Concat(scenarioContext.Vars)
                    .GroupBy(kvp => kvp.Key, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.Ordinal);
                scenarioStatus = stepResult.Status;
                scenarioException = stepResult.Exception;
            }
            else
            {
                stepResult = new StepResult<T>
                {
                    Status = ConfirmStatus.Indecisive,
                    State = StepState.Idle,
                    Vars = new Dictionary<string, object>(scenarioContext.Vars, StringComparer.Ordinal)
                };
            }

            stepResults.Add(stepResult);
        }

        ConfirmStepResult<T> result = new(scenarioStatus, stepResults, scenarioContext.Data,
            new Dictionary<string, object>(scenarioContext.Vars, StringComparer.Ordinal), scenarioException);

        return result;
    }
}
