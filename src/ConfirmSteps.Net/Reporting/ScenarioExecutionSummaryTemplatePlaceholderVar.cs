namespace ConfirmSteps.Reporting;

/// <summary>
/// Represents a placeholder for a variable in a scenario execution summary template.
/// </summary>
public sealed class ScenarioExecutionSummaryTemplatePlaceholderVar : ScenarioExecutionSummaryTemplatePlaceholderCommon
{
  private ScenarioExecutionSummaryTemplatePlaceholderVar()
    : base(string.Empty)
  {
  }

  /// <summary>
  /// Gets the singleton instance of the <see cref="ScenarioExecutionSummaryTemplatePlaceholderVar"/> class,
  /// which provides access to the variable placeholders for scenario execution summary templates.
  /// </summary>
  public static ScenarioExecutionSummaryTemplatePlaceholderVar Instance { get; } = new();

  /// <summary>
  /// Gets a placeholder for the variable name,
  /// which can be used in the template to indicate where to insert the name of a variable
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder VarName { get; } = new Placeholder("VarName");

  /// <summary>
  /// Gets a placeholder for the variable value,
  /// which can be used in the template to indicate where to insert the value of a variable
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder VarValue { get; } = new Placeholder("VarValue");

  private sealed class Placeholder : ScenarioExecutionSummaryTemplatePlaceholder
  {
    public Placeholder(string name)
      : base(name)
    {
    }
  }
}
