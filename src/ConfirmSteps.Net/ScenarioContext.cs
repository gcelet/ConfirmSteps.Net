namespace ConfirmSteps;

public sealed class ScenarioContext<T>
{
    public ScenarioContext(T data, IServiceProvider services)
    {
        Data = data;
        Services = services;
    }

    public T Data { get; }

    public IServiceProvider Services { get; }

    public Dictionary<string, object> Vars { get; set; } = new();
}