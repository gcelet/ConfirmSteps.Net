namespace ConfirmSteps;

public interface IScenarioBuilder<T>
    where T : class
{
    Scenario<T> Build();
}