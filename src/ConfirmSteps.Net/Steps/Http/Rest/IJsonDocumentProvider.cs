namespace ConfirmSteps.Steps.Http.Rest;

using System.Text.Json;

/// <summary>
/// Defines a provider that gives access to a <see cref="JsonDocument"/>.
/// </summary>
public interface IJsonDocumentProvider
{
    /// <summary>
    /// Gets the JSON document.
    /// </summary>
    JsonDocument? JsonDocument { get; }
}
