namespace ConfirmSteps.Reporting;

/// <summary>
/// Predefined templates for quick configuration of <see cref="ScenarioExecutionSummaryOptions"/> with common formatting styles.
/// </summary>
public enum ScenarioExecutionSummaryPredefinedTemplate
{
  /// <summary>
  /// Configures the summary to use the default templates with emoticons for status representation.
  /// This is the default configuration.
  /// </summary>
  Default,

  /// <summary>
  /// Configures the summary to use the default templates without emoticons for status representation.
  /// </summary>
  DefaultWithoutEmoticons
}
