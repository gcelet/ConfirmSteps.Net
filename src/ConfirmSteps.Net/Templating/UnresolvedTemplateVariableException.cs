namespace ConfirmSteps.Templating;

/// <summary>
/// Thrown when a request is built while one of the variables its templates expect has no value.
/// </summary>
/// <remarks>
/// <para>
/// Rendering a template leaves an unknown placeholder in place, which is the right answer for a
/// report — a summary line missing one value is still worth printing. It is the wrong answer for a
/// request: the placeholder goes out on the wire, url-encoded, and the server answers 400 or 404. The
/// caller then reads a broken correlation chain as the system under test misbehaving, which is the
/// most expensive kind of wrong.
/// </para>
/// <para>
/// Every missing variable is reported at once, with the part of the request that wanted it. A
/// descriptor missing three variables should say three, not send the author round the loop three
/// times.
/// </para>
/// </remarks>
public sealed class UnresolvedTemplateVariableException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnresolvedTemplateVariableException"/> class.
    /// </summary>
    public UnresolvedTemplateVariableException()
        : this([])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnresolvedTemplateVariableException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UnresolvedTemplateVariableException(string message)
        : base(message)
    {
        Unresolved = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnresolvedTemplateVariableException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of this one.</param>
    public UnresolvedTemplateVariableException(string message, Exception innerException)
        : base(message, innerException)
    {
        Unresolved = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnresolvedTemplateVariableException"/> class.
    /// </summary>
    /// <param name="unresolved">
    /// What could not be resolved: the part of the request, and the variable it expected.
    /// </param>
    public UnresolvedTemplateVariableException(IReadOnlyList<UnresolvedTemplateVariable> unresolved)
        : base(Describe(unresolved))
    {
        Unresolved = unresolved;
    }

    /// <summary>
    /// Gets what could not be resolved, so a host can report it its own way.
    /// </summary>
    public IReadOnlyList<UnresolvedTemplateVariable> Unresolved { get; }

    private static string Describe(IReadOnlyList<UnresolvedTemplateVariable> unresolved)
    {
        if (unresolved.Count == 0)
        {
            return "A request template expected a variable that has no value.";
        }

        string detail = string.Join(", ", unresolved.Select(u => $"{u.Name} (in {u.Location})"));

        return unresolved.Count == 1
            ? $"The request template expects a variable that has no value: {detail}."
            : $"The request templates expect {unresolved.Count} variables that have no value: {detail}.";
    }
}
