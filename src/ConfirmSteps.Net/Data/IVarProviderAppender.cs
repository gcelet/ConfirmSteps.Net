namespace ConfirmSteps.Data;

public interface IVarProviderAppender<T>
    where T : class
{
    void Append(VarProvider<T> varProvider);
}