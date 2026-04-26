namespace ConfirmSteps.Reporting;

/// <summary>
/// Represents a placeholder for global information in a scenario execution summary template.
/// </summary>
public sealed class ScenarioExecutionSummaryTemplatePlaceholderGlobal : ScenarioExecutionSummaryTemplatePlaceholderCommon
{
  private ScenarioExecutionSummaryTemplatePlaceholderGlobal()
    : base(string.Empty)
  {
  }

  /// <summary>
  /// Gets the singleton instance of the <see cref="ScenarioExecutionSummaryTemplatePlaceholderGlobal"/> class,
  /// which provides access to the global placeholders for scenario execution summary templates.
  /// </summary>
  public static ScenarioExecutionSummaryTemplatePlaceholderGlobal Instance { get; } = new();
}
