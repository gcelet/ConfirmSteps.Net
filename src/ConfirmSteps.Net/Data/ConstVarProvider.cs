namespace ConfirmSteps.Data;

public sealed class ConstVarProvider<T> : VarProvider<T>
    where T : class
{
    /// <inheritdoc />
    public ConstVarProvider(string key, object value)
        : base(key)
    {
        Value = value;
    }

    private object Value { get; }

    /// <inheritdoc />
    public override object ProviderValue(T data)
    {
        return Value;
    }
}