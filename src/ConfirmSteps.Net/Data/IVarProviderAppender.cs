namespace ConfirmSteps.Data;

/// <summary>
/// Defines an appender for variable providers.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public interface IVarProviderAppender<T>
    where T : class
{
    /// <summary>
    /// Appends a variable provider.
    /// </summary>
    /// <param name="varProvider">The variable provider to append.</param>
    void Append(VarProvider<T> varProvider);
}
