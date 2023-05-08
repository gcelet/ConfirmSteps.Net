namespace ConfirmSteps.Data;

public abstract class VarProvider<T>
    where T : class
{
    public VarProvider(string key)
    {
        Key = key;
    }

    public string Key { get; }

    public abstract object ProviderValue(T data);
}