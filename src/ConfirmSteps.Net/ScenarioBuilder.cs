namespace ConfirmSteps;

using ConfirmSteps.Data;
using ConfirmSteps.Internal;
using ConfirmSteps.Steps;
using Microsoft.Extensions.DependencyInjection;

public sealed class ScenarioBuilder<T> : IScenarioCustomizer<T>, IScenarioBuilder<T>, IStepBuilderAppender<T>
    where T : class
{
    public ScenarioBuilder(string title)
    {
        Title = title;
    }

    private VarBuilder<T> GlobalBuilder { get; } = new();

    private IServiceCollection Services { get; } = new ServiceCollection();

    private List<IStepBuilder<T>> StepBuilders { get; } = new();

    private string Title { get; }

    /// <inheritdoc />
    void IStepBuilderAppender<T>.Append(IStepBuilder<T> stepBuilder)
    {
        StepBuilders.Add(stepBuilder);
    }

    /// <inheritdoc />
    Scenario<T> IScenarioBuilder<T>.Build()
    {
        int nbStep = StepBuilders.Count;
        VarManager<T> varManager = new(((IVarProviderConverter<T>)GlobalBuilder).ToVarProviders());
        IServiceCollection services = Services;

        services.AddSingleton(varManager);

        for (int i = 0; i < nbStep; i++)
        {
            IStepBuilder<T> stepBuilder = StepBuilders[i];

            services = stepBuilder.RegisterServices(services);
        }

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        List<IStep<T>> steps = new();

        for (int i = 0; i < nbStep; i++)
        {
            IStepBuilder<T> stepBuilder = StepBuilders[i];

            IStep<T> step = stepBuilder.Build();

            steps.Add(step);
        }

        Scenario<T> scenario = new(Title, steps, serviceProvider);

        return scenario;
    }

    /// <inheritdoc />
    IScenarioCustomizer<T> IScenarioCustomizer<T>.WithGlobals(Action<VarBuilder<T>> globalBuilder)
    {
        globalBuilder.Invoke(GlobalBuilder);
        return this;
    }

    /// <inheritdoc />
    IScenarioCustomizer<T> IScenarioCustomizer<T>.WithServices(Action<IServiceCollection> services)
    {
        services.Invoke(Services);
        return this;
    }

    /// <inheritdoc />
    IScenarioBuilder<T> IScenarioCustomizer<T>.WithSteps(Action<IStepBuilderAppender<T>> stepBuilderAppender)
    {
        stepBuilderAppender.Invoke(this);
        return this;
    }
}