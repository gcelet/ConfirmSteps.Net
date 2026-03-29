namespace ConfirmSteps;

/// <summary>
/// Provides contextual information for a scenario during its execution.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class ScenarioContext<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioContext{T}"/> class.
    /// </summary>
    /// <param name="data">The initial data object.</param>
    /// <param name="services">The service provider.</param>
    public ScenarioContext(T data, IServiceProvider services)
    {
        Data = data;
        Services = services;
    }

    /// <summary>
    /// Gets the data object.
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets or sets the variables collected during execution.
    /// </summary>
    public Dictionary<string, object> Vars { get; set; } = new();
}
