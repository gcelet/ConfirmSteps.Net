namespace ConfirmSteps.Steps.Http.Json;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Thrown when a step description cannot be turned into a step.
/// </summary>
/// <remarks>
/// Raised while the scenario is being built, never during a run. A description is read once and a
/// mistake in it is a mistake in the file, so it belongs to whoever loads it — not to the report of a
/// run that got half way.
/// </remarks>
[SuppressMessage("Design", "CA1032", Justification = "The standard message constructors are left out deliberately: every instance is raised by one of the named factories below, each of which knows how to say what is wrong and what would be right. A free-text overload would let a caller raise one that explains neither.")]
public sealed class HttpStepDescriptionException : InvalidOperationException
{
    private HttpStepDescriptionException(string message)
        : base(message)
    {
    }

    /// <summary>A property the description cannot do without is absent or of the wrong shape.</summary>
    /// <param name="path">Where in the description the problem is.</param>
    /// <param name="expected">What was expected there.</param>
    public static HttpStepDescriptionException Invalid(string path, string expected)
        => new($"The step description is not usable at {path}: {expected}.");

    /// <summary>The description asks for a verification the host did not register.</summary>
    /// <param name="kind">The name the description used.</param>
    /// <param name="registered">The names that are available.</param>
    public static HttpStepDescriptionException UnknownVerificationKind(string kind,
        IReadOnlyCollection<string> registered)
        => new($"The step description asks for the verification '{kind}', which nothing registered. "
               + (registered.Count == 0
                   ? "No verification kind is registered at all: a host supplies them, because what a "
                     + "response ought to contain is its judgement to make."
                   : $"Registered kinds: {string.Join(", ", registered)}."));

    /// <summary>The description names an extraction kind that does not exist.</summary>
    /// <param name="kind">The name the description used.</param>
    public static HttpStepDescriptionException UnknownExtractionKind(string kind)
        => new($"The step description asks to extract '{kind}', which is not a known kind. "
               + $"Known kinds: {string.Join(", ", Enum.GetNames<ExtractedValueKind>())}.");
}
