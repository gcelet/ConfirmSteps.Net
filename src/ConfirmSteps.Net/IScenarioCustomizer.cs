namespace ConfirmSteps;

using ConfirmSteps.Data;
using Microsoft.Extensions.DependencyInjection;

public interface IScenarioCustomizer<T>
    where T : class
{
    IScenarioCustomizer<T> WithGlobals(Action<VarBuilder<T>> globalBuilder);

    IScenarioCustomizer<T> WithServices(Action<IServiceCollection> services);

    IScenarioBuilder<T> WithSteps(Action<IStepBuilderAppender<T>> stepBuilderAppender);
}