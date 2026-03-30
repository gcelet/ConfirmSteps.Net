namespace ConfirmSteps.Data;

/// <summary>
/// Provides a value for a scenario variable.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public abstract class VarProvider<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VarProvider{T}"/> class.
    /// </summary>
    /// <param name="key">The key (name) of the variable.</param>
    protected VarProvider(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets the key of the variable.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Provides the value for the variable given the scenario data.
    /// </summary>
    /// <param name="data">The scenario data.</param>
    /// <returns>The value for the variable.</returns>
    public abstract object ProviderValue(T data);
}
