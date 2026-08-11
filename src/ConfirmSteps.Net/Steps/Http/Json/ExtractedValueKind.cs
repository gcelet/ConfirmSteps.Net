namespace ConfirmSteps.Steps.Http.Json;

/// <summary>
/// What a described extraction reads out of a response.
/// </summary>
/// <remarks>
/// The kind is declared rather than guessed from the JSON, because the two differ in ways that
/// matter: an identifier a server writes as a number is usually wanted as a string for the next
/// request's path, and a path matching one element is not the same intent as a path matching a list.
/// </remarks>
public enum ExtractedValueKind
{
    /// <summary>One string.</summary>
    String = 0,

    /// <summary>Every string the path matches, in document order.</summary>
    StringList,

    /// <summary>One number.</summary>
    Number,

    /// <summary>Every number the path matches, in document order.</summary>
    NumberList,

    /// <summary>One boolean.</summary>
    Boolean,

    /// <summary>Every boolean the path matches, in document order.</summary>
    BooleanList,
}
