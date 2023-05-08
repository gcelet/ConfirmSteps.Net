namespace ConfirmSteps.Data;

public sealed class VarBuilder<T> : IVarProviderAppender<T>, IVarProviderConverter<T> where T : class
{
    private List<VarProvider<T>> VarProviders { get; } = new();

    public VarBuilder<T> UseConst(string key, object value)
    {
        VarProviders.Add(new ConstVarProvider<T>(key, value));
        return this;
    }

    public VarBuilder<T> UseObject(string key, Func<T, object> extractor)
    {
        VarProviders.Add(new ObjectVarProvider<T>(key, extractor));
        return this;
    }

    /// <inheritdoc />
    void IVarProviderAppender<T>.Append(VarProvider<T> varProvider)
    {
        VarProviders.Add(varProvider);
    }

    /// <inheritdoc />
    VarProvider<T>[] IVarProviderConverter<T>.ToVarProviders()
    {
        return VarProviders.ToArray();
    }
}