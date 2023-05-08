namespace ConfirmSteps;

using ConfirmSteps.Data;
using ConfirmSteps.Steps;
using Microsoft.Extensions.DependencyInjection;

public static class Scenario
{
    public static IScenarioCustomizer<T> New<T>(string title)
        where T : class
    {
        return new ScenarioBuilder<T>(title);
    }
}

public sealed class Scenario<T>
    where T : class
{
    public Scenario(string title, IReadOnlyList<IStep<T>> steps, IServiceProvider services)
    {
        Title = title;
        Steps = steps;
        Services = services;
    }

    public string Title { get; }

    private IServiceProvider Services { get; }

    private IReadOnlyList<IStep<T>> Steps { get; }

    public async Task<ConfirmStepResult<T>> ConfirmSteps(T data, CancellationToken cancellationToken)
    {
        int nbSteps = Steps.Count;
        VarManager<T> varManager = Services.GetRequiredService<VarManager<T>>();
        IReadOnlyDictionary<string, object> globalVars = varManager.Extract(data);
        IServiceScopeFactory serviceScopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
        ScenarioContext<T> scenarioContext = new(data, Services)
        {
            Vars = new Dictionary<string, object>(globalVars)
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
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.First().Value);
                scenarioStatus = stepResult.Status;
                scenarioException = stepResult.Exception;
            }
            else
            {
                stepResult = new StepResult<T>
                {
                    Status = ConfirmStatus.Indecisive,
                    State = StepState.Idle,
                    Vars = new Dictionary<string, object>(scenarioContext.Vars)
                };
            }

            stepResults.Add(stepResult);
        }

        ConfirmStepResult<T> result = new(scenarioStatus, stepResults, scenarioContext.Data,
            new Dictionary<string, object>(scenarioContext.Vars), scenarioException);

        return result;
    }
}