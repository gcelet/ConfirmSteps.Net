namespace ConfirmSteps.Steps.Http.RequestBuilding;

using ConfirmSteps.Templating;

/// <summary>
/// Provides a builder for configuring HTTP request headers.
/// </summary>
public sealed class HeaderBuilder
{
    internal Dictionary<TemplateString, TemplateString> Headers { get; } = new();

    /// <summary>
    /// Adds or updates an HTTP header.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header.</param>
    /// <returns>The current <see cref="HeaderBuilder"/> for fluent chaining.</returns>
    public HeaderBuilder Header(TemplateString name, TemplateString value)
    {
        Headers[name] = value;

        return this;
    }
}
