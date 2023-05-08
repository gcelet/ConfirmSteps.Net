namespace ConfirmSteps.Steps.Http.RequestBuilding;

using ConfirmSteps.Templating;

public sealed class QueryStringBuilder
{
    internal List<KeyValuePair<TemplateString, TemplateString>> QueryString { get; } = new();

    public QueryStringBuilder Append(TemplateString key, TemplateString value)
    {
        QueryString.Add(new KeyValuePair<TemplateString, TemplateString>(key, value));

        return this;
    }
}