namespace ConfirmSteps.Data;

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