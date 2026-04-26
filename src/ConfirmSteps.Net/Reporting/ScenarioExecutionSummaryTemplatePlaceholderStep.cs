namespace ConfirmSteps.Reporting;

/// <summary>
/// Represents a placeholder for step information in a scenario execution summary template.
/// </summary>
public sealed class ScenarioExecutionSummaryTemplatePlaceholderStep : ScenarioExecutionSummaryTemplatePlaceholderCommon
{
  private ScenarioExecutionSummaryTemplatePlaceholderStep()
    : base(string.Empty)
  {
  }

  /// <summary>
  /// Gets the singleton instance of the <see cref="ScenarioExecutionSummaryTemplatePlaceholderStep"/> class,
  /// which provides access to the step information placeholders for scenario execution summary templates.
  /// </summary>
  public static ScenarioExecutionSummaryTemplatePlaceholderStep Instance { get; } = new();

  /// <summary>
  /// Gets a placeholder for the step number,
  /// which can be used in the template to indicate where to insert the number of the step being described
  /// (e.g., "1" for the first step, "2" for the second step, etc.).
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepNumber { get; } = new Placeholder("StepNumber");

  /// <summary>
  /// Gets a placeholder for the step state emoticon,
  /// which can be used in the template to indicate where to insert an emoticon
  /// representing the state of the step being described (e.g., idle, prepared, executing, verifying, extracting, completed).
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepStateEmoticon { get; } = new Placeholder("StepStateEmoticon");

  /// <summary>
  /// Gets a placeholder for the step state text,
  /// which can be used in the template to indicate where to insert a human-readable text description
  /// of the state of the step being described (e.g., "Idle", "Prepared", "Executing", "Verifying", "Extracting", "Completed").
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepStateText { get; } = new Placeholder("StepStateText");

  /// <summary>
  /// Gets a placeholder for the step status emoticon,
  /// which can be used in the template to indicate where to insert an emoticon
  /// representing the status of the step being described (e.g., passed, failed, indecisive).
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepStatusEmoticon { get; } = new Placeholder("StepStatusEmoticon");

  /// <summary>
  /// Gets a placeholder for the step status text,
  /// which can be used in the template to indicate where to insert a human-readable text description
  /// of the status of the step being described (e.g., "Passed", "Failed", "Indecisive").
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepStatusText { get; } = new Placeholder("StepStatusText");

  /// <summary>
  /// Gets a placeholder for the step title,
  /// which can be used in the template to indicate where to insert the title of the step being described.
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepTitle { get; } = new Placeholder("StepTitle");

  private sealed class Placeholder : ScenarioExecutionSummaryTemplatePlaceholder
  {
    public Placeholder(string name)
      : base(name)
    {
    }
  }
}
