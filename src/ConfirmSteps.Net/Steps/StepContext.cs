namespace ConfirmSteps.Steps;

/// <summary>
/// Provides contextual information for a step during its execution.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class StepContext<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StepContext{T}"/> class.
    /// </summary>
    /// <param name="scenarioContext">The parent scenario context.</param>
    /// <param name="services">The service provider for the current scope.</param>
    /// <param name="vars">The current set of variables.</param>
    public StepContext(ScenarioContext<T> scenarioContext, IServiceProvider services,
        IReadOnlyDictionary<string, object> vars)
    {
        ScenarioContext = scenarioContext;
        Services = services;
        Vars = new Dictionary<string, object>(vars);
    }

    /// <summary>
    /// Gets a dictionary of items that can be used to share data between phases of a step.
    /// </summary>
    public Dictionary<string, object> Items { get; } = new();

    /// <summary>
    /// Gets the parent scenario context.
    /// </summary>
    public ScenarioContext<T> ScenarioContext { get; }

    /// <summary>
    /// Gets the service provider for the current scope.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets the variables collected or modified during execution.
    /// </summary>
    public Dictionary<string, object> Vars { get; }

    /// <summary>
    /// Adds an item to the context with a specific key.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="itemKey">The key for the item.</param>
    /// <param name="item">The item to add.</param>
    public void AddItem<TItem>(string itemKey, TItem item)
        where TItem : class
    {
        Items[itemKey] = item;
    }

    /// <summary>
    /// Adds an item to the context using its type name as the key.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="item">The item to add.</param>
    public void AddItem<TItem>(TItem item)
        where TItem : class
    {
        AddItem(typeof(TItem).FullName!, item);
    }

    /// <summary>
    /// Tries to get an item from the context with a specific key.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="itemKey">The key of the item.</param>
    /// <param name="item">When this method returns, contains the item associated with the specified key, if the key is found and the item is of the correct type; otherwise, null.</param>
    /// <returns>true if the item was found and is of the correct type; otherwise, false.</returns>
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

    /// <summary>
    /// Tries to get an item from the context using its type name as the key.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="item">When this method returns, contains the item associated with the type name, if found and of the correct type; otherwise, null.</param>
    /// <returns>true if the item was found; otherwise, false.</returns>
    public bool TryGetItem<TItem>(out TItem? item)
        where TItem : class
    {
        return TryGetItem(typeof(TItem).FullName!, out item);
    }
}
