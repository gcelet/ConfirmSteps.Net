namespace ConfirmSteps.Data;

/// <summary>
/// Provides a value extracted from the scenario data for a variable.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class ObjectVarProvider<T> : VarProvider<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectVarProvider{T}"/> class.
    /// </summary>
    /// <param name="key">The key of the variable to provide.</param>
    /// <param name="extractor">The function to extract the value from the scenario data.</param>
    public ObjectVarProvider(string key, Func<T, object> extractor)
        : base(key)
    {
        Extractor = extractor;
    }

    private Func<T, object> Extractor { get; }

    /// <inheritdoc />
    public override object ProviderValue(T data)
    {
        return Extractor(data);
    }
}
