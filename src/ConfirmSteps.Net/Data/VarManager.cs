namespace ConfirmSteps.Data;

/// <summary>
/// Manages variable extraction from scenario data using a collection of variable providers.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public sealed class VarManager<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VarManager{T}"/> class.
    /// </summary>
    /// <param name="varProviders">The variable providers to use for extraction.</param>
    public VarManager(params VarProvider<T>[] varProviders)
    {
        VarProviders = varProviders;
    }

    private VarProvider<T>[] VarProviders { get; }

    /// <summary>
    /// Extracts variables from the specified data.
    /// </summary>
    /// <param name="data">The scenario data to extract from.</param>
    /// <returns>A dictionary of extracted variables.</returns>
    public IReadOnlyDictionary<string, object> Extract(T data)
    {
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        foreach (VarProvider<T> varProvider in VarProviders)
        {
            object value = varProvider.ProviderValue(data);

            vars[varProvider.Key] = value;
        }

        return vars;
    }
}
