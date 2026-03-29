namespace ConfirmSteps.Data;

/// <summary>
/// Defines a converter that provides an array of variable providers.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public interface IVarProviderConverter<T>
    where T : class
{
    /// <summary>
    /// Converts to an array of variable providers.
    /// </summary>
    /// <returns>An array of variable providers.</returns>
    VarProvider<T>[] ToVarProviders();
}
