namespace ConfirmSteps.Reporting;

/// <summary>
/// Represents common placeholders that can be used in scenario execution summary templates,
/// such as scenario title, status, and step counts.
/// </summary>
public abstract class ScenarioExecutionSummaryTemplatePlaceholderCommon : ScenarioExecutionSummaryTemplatePlaceholder
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ScenarioExecutionSummaryTemplatePlaceholderCommon"/> class with the specified name.
  /// </summary>
  /// <param name="name">
  /// The name of the placeholder group, which can be used to identify the context of the placeholders in the template.
  /// </param>
  protected ScenarioExecutionSummaryTemplatePlaceholderCommon(string name)
    : base(name)
  {
  }

  /// <summary>
  /// Gets a placeholder for the exception message,
  /// which can be used in the template to indicate where to insert the message of any exception
  /// that occurred during scenario execution.
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder ExceptionMessage { get; } = new Placeholder("ExceptionMessage");

  /// <summary>
  /// Gets a placeholder for the exception type,
  /// which can be used in the template to indicate where to insert the type of any exception
  /// that occurred during scenario execution.
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder ExceptionType { get; } = new Placeholder("ExceptionType");

  /// <summary>
  /// Gets a placeholder for the scenario status emoticon,
  /// which can be used in the template to indicate where to insert an emoticon
  /// representing the overall status of the scenario execution (e.g., passed, failed, indecisive).
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder ScenarioStatusEmoticon { get; } = new Placeholder("ScenarioStatusEmoticon");

  /// <summary>
  /// Gets a placeholder for the scenario status text,
  /// which can be used in the template to indicate where to insert a human-readable text description
  /// of the overall status of the scenario execution (e.g., "Passed", "Failed", "Indecisive").
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder ScenarioStatusText { get; } = new Placeholder("ScenarioStatusText");

  /// <summary>
  /// Gets a placeholder for the scenario title,
  /// which can be used in the template to indicate where to insert the title of the scenario being executed.
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder ScenarioTitle { get; } = new Placeholder("ScenarioTitle");

  /// <summary>
  /// Gets a placeholder for the total step count,
  /// which can be used in the template to indicate where to insert the total number of steps of the scenario.
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepCount { get; } = new Placeholder("StepCount");

  /// <summary>
  /// Gets a placeholder for the failed step count,
  /// which can be used in the template to indicate where to insert the number of steps that failed during the scenario execution.
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepFailedCount { get; } = new Placeholder("StepFailedCount");

  /// <summary>
  /// Gets a placeholder for the indecisive step count,
  /// which can be used in the template to indicate where to insert the number of steps that were indecisive
  /// (neither passed nor failed) during the scenario execution.
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepIndecisiveCount { get; } = new Placeholder("StepIndecisiveCount");

  /// <summary>
  /// Gets a placeholder for the passed step count,
  /// which can be used in the template to indicate where to insert the number of steps that passed successfully
  /// during the scenario execution.
  /// </summary>
  public ScenarioExecutionSummaryTemplatePlaceholder StepPassedCount { get; } = new Placeholder("StepPassedCount");

  private sealed class Placeholder : ScenarioExecutionSummaryTemplatePlaceholder
  {
    public Placeholder(string name)
      : base(name)
    {
    }
  }
}
