namespace ConfirmSteps.Steps.Http.Json;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Thrown when an extraction a description marked required finds nothing.
/// </summary>
/// <remarks>
/// Extraction used to be silent: a path matching nothing set no variable and said nothing about it.
/// The step that needed the value then built its request with an unresolved placeholder, so the
/// failure surfaced as a server error several steps later. Chaining a value from one step to another is
/// what this library is for, and a broken chain is worth saying out loud where it breaks.
/// </remarks>
[SuppressMessage("Design", "CA1032", Justification = "The standard message constructors are left out deliberately: this failure is only meaningful for a named variable and the path that was meant to feed it, and a free-text overload would let a caller raise one that identifies neither.")]
public sealed class RequiredExtractionFailedException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredExtractionFailedException"/> class.
    /// </summary>
    /// <param name="variableName">Variable the extraction was meant to set.</param>
    /// <param name="path">Path that matched nothing.</param>
    public RequiredExtractionFailedException(string variableName, string path)
        : base($"The required extraction of {variableName} found nothing at '{path}'. The steps that "
               + "consume it would otherwise send an unresolved placeholder, and the failure would "
               + "surface as a server error somewhere else.")
    {
        VariableName = variableName;
        Path = path;
    }

    /// <summary>Gets the variable the extraction was meant to set.</summary>
    public string VariableName { get; }

    /// <summary>Gets the path that matched nothing.</summary>
    public string Path { get; }
}
