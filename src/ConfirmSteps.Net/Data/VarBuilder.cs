namespace ConfirmSteps.Data;

/// <summary>
/// Provides a builder for configuring variables within a scenario.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class VarBuilder<T> : IVarProviderAppender<T>, IVarProviderConverter<T> where T : class
{
    private List<VarProvider<T>> VarProviders { get; } = new();

    /// <summary>
    /// Adds a constant variable to the scenario.
    /// </summary>
    /// <param name="key">The key of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <returns>The current <see cref="VarBuilder{T}"/> for fluent chaining.</returns>
    public VarBuilder<T> UseConst(string key, object value)
    {
        VarProviders.Add(new ConstVarProvider<T>(key, value));
        return this;
    }

    /// <summary>
    /// Adds a variable extracted from the data object.
    /// </summary>
    /// <param name="key">The key of the variable.</param>
    /// <param name="extractor">A function to extract the value from the data object.</param>
    /// <returns>The current <see cref="VarBuilder{T}"/> for fluent chaining.</returns>
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
