namespace ConfirmSteps.Reporting;

/// <summary>
/// Represents a placeholder in a scenario execution summary template.
/// Placeholders are used to indicate where specific pieces of information about the scenario execution should be inserted
/// when generating the summary string.
/// </summary>
public abstract class ScenarioExecutionSummaryTemplatePlaceholder
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ScenarioExecutionSummaryTemplatePlaceholder"/> class with the specified name.
  /// </summary>
  /// <param name="name">
  /// The name of the placeholder, which will be used in the template string to identify where to insert the corresponding value.
  /// </param>
  protected ScenarioExecutionSummaryTemplatePlaceholder(string name)
  {
    Name = name;
  }

  /// <summary>
  /// Defines an implicit conversion from a <see cref="ScenarioExecutionSummaryTemplatePlaceholder"/> to a string.
  /// </summary>
  /// <param name="p">The placeholder to convert to a string.</param>
  /// <returns>>A string representation of the placeholder, formatted as "{{PlaceholderName}}".</returns>
  public static implicit operator string(ScenarioExecutionSummaryTemplatePlaceholder p)
  {
    return p.ToString();
  }

  /// <summary>
  /// Gets the name of the placeholder, which is used in the template string to identify where to insert the corresponding value.
  /// </summary>
  public string Name { get; }

  /// <inheritdoc />
  public override string ToString() => $"{{{{{Name}}}}}";
}
