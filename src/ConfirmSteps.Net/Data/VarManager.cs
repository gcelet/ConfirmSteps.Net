namespace ConfirmSteps.Data;

public sealed class VarManager<T>
    where T : class
{
    public VarManager(params VarProvider<T>[] varProviders)
    {
        VarProviders = varProviders;
    }

    private VarProvider<T>[] VarProviders { get; }

    public IReadOnlyDictionary<string, object> Extract(T data)
    {
        Dictionary<string, object> vars = new();

        foreach (VarProvider<T> varProvider in VarProviders)
        {
            object value = varProvider.ProviderValue(data);

            vars[varProvider.Key] = value;
        }

        return vars;
    }
}