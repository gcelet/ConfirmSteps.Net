namespace ConfirmSteps.Steps;

public sealed class StepContext<T>
    where T : class
{
    public StepContext(ScenarioContext<T> scenarioContext, IServiceProvider services,
        IReadOnlyDictionary<string, object> vars)
    {
        ScenarioContext = scenarioContext;
        Services = services;
        Vars = new Dictionary<string, object>(vars);
    }

    public Dictionary<string, object> Items { get; } = new();

    public ScenarioContext<T> ScenarioContext { get; }

    public IServiceProvider Services { get; }

    public Dictionary<string, object> Vars { get; }

    public void AddItem<TItem>(string itemKey, TItem item)
        where TItem : class
    {
        Items[itemKey] = item;
    }

    public void AddItem<TItem>(TItem item)
        where TItem : class
    {
        AddItem(typeof(TItem).FullName!, item);
    }

    public bool TryGetItem<TItem>(string itemKey, out TItem? item)
        where TItem : class
    {
        if (!Items.TryGetValue(itemKey, out object? objectItem) || objectItem is not TItem i)
        {
            item = default;
            return false;
        }

        item = i;
        return true;
    }

    public bool TryGetItem<TItem>(out TItem? item)
        where TItem : class
    {
        return TryGetItem(typeof(TItem).FullName!, out item);
    }
}