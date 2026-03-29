namespace ConfirmSteps.Steps.Http.RequestBuilding;

using ConfirmSteps.Templating;

/// <summary>
/// Provides a builder for configuring the query string of an HTTP request.
/// </summary>
public sealed class QueryStringBuilder
{
    internal List<KeyValuePair<TemplateString, TemplateString>> QueryString { get; } = new();

    /// <summary>
    /// Appends a key-value pair to the query string.
    /// </summary>
    /// <param name="key">The key of the query parameter.</param>
    /// <param name="value">The value of the query parameter.</param>
    /// <returns>The current <see cref="QueryStringBuilder"/> for fluent chaining.</returns>
    public QueryStringBuilder Append(TemplateString key, TemplateString value)
    {
        QueryString.Add(new KeyValuePair<TemplateString, TemplateString>(key, value));

        return this;
    }
}
