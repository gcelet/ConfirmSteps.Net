namespace ConfirmSteps.Data;

/// <summary>
/// Provides a value extracted from the scenario data for a variable.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class ObjectVarProvider<T> : VarProvider<T>
    where T : class
{
    /// <inheritdoc />
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
