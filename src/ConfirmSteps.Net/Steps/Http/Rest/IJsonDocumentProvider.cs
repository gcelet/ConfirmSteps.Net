namespace ConfirmSteps.Steps.Http.Rest;

using System.Text.Json;

public interface IJsonDocumentProvider
{
    JsonDocument? JsonDocument { get; }
}