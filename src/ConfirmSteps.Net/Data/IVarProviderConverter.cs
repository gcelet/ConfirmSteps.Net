namespace ConfirmSteps.Data;

public interface IVarProviderConverter<T>
    where T : class
{
    VarProvider<T>[] ToVarProviders();
}