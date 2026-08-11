namespace ConfirmSteps.Templating;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Thrown when a variable carrying several values is used where only text can be produced.
/// </summary>
/// <remarks>
/// A query parameter whose value is exactly one placeholder can repeat, once per value the variable
/// holds. Text built around a placeholder cannot: <c>ids-{{MODEL_IDS}}</c> has no reading that makes
/// sense for a list. Before, such a template rendered the collection through <c>ToString</c> and sent
/// <c>ids-System.String[]</c>, which the server rejected for reasons that pointed nowhere near the
/// mistake.
/// </remarks>
[SuppressMessage("Design", "CA1032", Justification = "The standard message constructors are left out deliberately: this exception is only meaningful for a named variable, and a free-text overload would let a caller raise one that names nothing — which is the single piece of information needed to fix the template.")]
public sealed class MultiValuedTemplateVariableException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiValuedTemplateVariableException"/> class.
    /// </summary>
    /// <param name="variableName">Name of the variable that carries several values.</param>
    public MultiValuedTemplateVariableException(string variableName)
        : base($"The variable {variableName} carries several values, so it can only be used as the "
               + "entire value of a query parameter, not inside surrounding text.")
    {
        VariableName = variableName;
    }

    /// <summary>
    /// Gets the name of the variable that carries several values.
    /// </summary>
    public string VariableName { get; }
}
