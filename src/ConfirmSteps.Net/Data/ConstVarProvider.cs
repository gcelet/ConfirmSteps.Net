namespace ConfirmSteps.Data;

/// <summary>
/// Provides a constant value for a scenario variable.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class ConstVarProvider<T> : VarProvider<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstVarProvider{T}"/> class with a constant value.
    /// </summary>
    /// <param name="key">The key of the variable to provide.</param>
    /// <param name="value">The constant value to provide.</param>
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
