namespace ConfirmSteps.Steps.Http.RequestBuilding;

using ConfirmSteps.Templating;

public sealed class HeaderBuilder
{
    internal Dictionary<TemplateString, TemplateString> Headers { get; } = new();

    public HeaderBuilder Header(TemplateString name, TemplateString value)
    {
        Headers[name] = value;

        return this;
    }
}