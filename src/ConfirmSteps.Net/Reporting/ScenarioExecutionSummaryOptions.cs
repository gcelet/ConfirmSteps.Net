namespace ConfirmSteps.Reporting;

using ConfirmSteps.Templating;

/// <summary>
/// Fluent options for configuring a <see cref="ScenarioExecutionSummaryReporter"/>.
/// </summary>
public sealed class ScenarioExecutionSummaryOptions
{
  private const string DefaultTemplateResultException = "  Exception: {{ExceptionMessage}}";

  private const string DefaultTemplateResultSummaryEmoticonDisabled =
    "Scenario \"{{ScenarioTitle}}\" — Status: {{ScenarioStatusText}}";

  private const string DefaultTemplateResultSummaryEmoticonEnabled =
    "Scenario \"{{ScenarioTitle}}\" — Status: {{ScenarioStatusEmoticon}} {{ScenarioStatusText}}";

  private const string DefaultTemplateStepException = "      Exception: {{ExceptionMessage}}";

  private const string DefaultTemplateStepsSummary = "Steps: [{{StepPassedCount}}/{{StepCount}}]";

  private const string DefaultTemplateStepSummaryEmoticonDisabled =
    "  ⁃{{StepNumber}}: [{{StepStatusText}}] {{StepTitle}}  (State: {{StepStateText}})";

  private const string DefaultTemplateStepSummaryEmoticonEnabled =
    "  ⁃{{StepNumber}}: [{{StepStatusEmoticon}} {{StepStatusText}}] {{StepTitle}}  (State: {{StepStateEmoticon}} {{StepStateText}})";

  private const string DefaultTemplateVar = "  {{VarName}}: {{VarValue}}";

  private const string DefaultTemplateVars = "Vars:";

  private static Dictionary<ConfirmStatus, string> DefaultConfirmStatusEmoticons { get; } = new()
  {
    [ConfirmStatus.Success] = "✅",
    [ConfirmStatus.Failure] = "❌",
    [ConfirmStatus.Indecisive] = "⏭️"
  };

  private static Dictionary<ConfirmStatus, string> DefaultConfirmStatusTexts { get; } = new()
  {
    [ConfirmStatus.Success] = "Pass",
    [ConfirmStatus.Failure] = "Fail",
    [ConfirmStatus.Indecisive] = "Skip"
  };

  private static Dictionary<StepState, string> DefaultStepStateEmoticons { get; } = new()
  {
    [StepState.Idle] = "⏸️",
    [StepState.Prepare] = "🔄",
    [StepState.Execute] = "⚡",
    [StepState.Verify] = "🔍",
    [StepState.Extract] = "📤",
    [StepState.Done] = "✅"
  };

  private static Dictionary<StepState, string> DefaultStepStateTexts { get; } = new()
  {
    [StepState.Idle] = "Idle",
    [StepState.Prepare] = "Preparing",
    [StepState.Execute] = "Executing",
    [StepState.Verify] = "Verifying",
    [StepState.Extract] = "Extracting",
    [StepState.Done] = "Done"
  };

  internal Dictionary<ConfirmStatus, string> ConfirmStatusEmoticons { get; } = new(DefaultConfirmStatusEmoticons);

  internal Dictionary<ConfirmStatus, string> ConfirmStatusTexts { get; } = new(DefaultConfirmStatusTexts);

  internal bool ShouldIncludePassedSteps { get; private set; } = true;

  internal bool ShouldIncludeResultException { get; private set; } = true;

  internal bool ShouldIncludeResultSummary { get; private set; } = true;

  internal bool ShouldIncludeSkippedSteps { get; private set; } = true;

  internal bool ShouldIncludeStepException { get; private set; } = true;

  internal bool ShouldIncludeSteps { get; private set; } = true;

  internal bool ShouldIncludeStepsSummary { get; private set; } = true;

  internal bool ShouldIncludeVars { get; private set; }

  internal Dictionary<StepState, string> StepStateEmoticons { get; } = new(DefaultStepStateEmoticons);

  internal Dictionary<StepState, string> StepStateTexts { get; } = new(DefaultStepStateTexts);

  internal TemplateString TemplateResultException { get; private set; } = DefaultTemplateResultException;

  internal TemplateString TemplateResultSummary { get; private set; } = DefaultTemplateResultSummaryEmoticonEnabled;

  internal TemplateString TemplateStepException { get; private set; } = DefaultTemplateStepException;

  internal TemplateString TemplateStepsSummary { get; private set; } = DefaultTemplateStepsSummary;

  internal TemplateString TemplateStepSummary { get; private set; } = DefaultTemplateStepSummaryEmoticonEnabled;

  internal TemplateString TemplateVar { get; private set; } = DefaultTemplateVar;

  internal TemplateString TemplateVars { get; private set; } = DefaultTemplateVars;

  /// <summary>
  /// Excludes steps that passed successfully from the summary.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions ExcludePassedSteps()
  {
    ShouldIncludePassedSteps = false;
    return this;
  }

  /// <summary>
  /// Excludes result exception details for the overall scenario from the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions ExcludeResultException()
  {
    ShouldIncludeResultException = false;
    return this;
  }

  /// <summary>
  /// Excludes the overall scenario result summary from the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions ExcludeResultSummary()
  {
    ShouldIncludeResultSummary = false;
    return this;
  }

  /// <summary>
  /// Excludes steps that were skipped (Indecisive) from the summary.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions ExcludeSkippedSteps()
  {
    ShouldIncludeSkippedSteps = false;
    return this;
  }

  /// <summary>
  /// Excludes step exception details for failed steps from the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions ExcludeStepException()
  {
    ShouldIncludeStepException = false;
    return this;
  }

  /// <summary>
  /// Excludes steps from the summary.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions ExcludeSteps()
  {
    ShouldIncludeSteps = false;
    return this;
  }

  /// <summary>
  /// Excludes the steps summary (e.g., "[1/2]") from the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions ExcludeStepsSummary()
  {
    ShouldIncludeStepsSummary = false;
    return this;
  }

  /// <summary>
  /// Excludes the variables collected during execution from the summary.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions ExcludeVars()
  {
    ShouldIncludeVars = false;
    return this;
  }

  /// <summary>
  /// Includes steps that passed successfully in the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions IncludePassedSteps()
  {
    ShouldIncludePassedSteps = true;
    return this;
  }

  /// <summary>
  /// Includes result exception details for failed steps in the summary.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions IncludeResultException()
  {
    ShouldIncludeResultException = true;
    return this;
  }

  /// <summary>
  /// Includes the overall scenario result summary in the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions IncludeResultSummary()
  {
    ShouldIncludeResultSummary = true;
    return this;
  }

  /// <summary>
  /// Includes steps that were skipped (Indecisive) in the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions IncludeSkippedSteps()
  {
    ShouldIncludeSkippedSteps = true;
    return this;
  }

  /// <summary>
  /// Includes step exception details for failed steps in the summary.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions IncludeStepException()
  {
    ShouldIncludeStepException = true;
    return this;
  }

  /// <summary>
  /// Includes steps in the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions IncludeSteps()
  {
    ShouldIncludeSteps = true;
    return this;
  }

  /// <summary>
  /// Includes the steps summary (e.g., "[1/2]") in the summary. Included by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions IncludeStepsSummary()
  {
    ShouldIncludeStepsSummary = true;
    return this;
  }

  /// <summary>
  /// Includes the variables collected during execution in the summary. Excluded by default.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions IncludeVars()
  {
    ShouldIncludeVars = true;
    return this;
  }

  /// <summary>
  /// Configures the emoticon representation for a given <see cref="ConfirmStatus"/> in the summary.
  /// </summary>
  /// <param name="status">The <see cref="ConfirmStatus"/> to configure.</param>
  /// <param name="emoticon">The emoticon to use for the specified status in the summary.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseEmoticonFor(ConfirmStatus status, string emoticon)
  {
    ConfirmStatusEmoticons[status] = emoticon;
    return this;
  }

  /// <summary>
  /// Configures the emoticon representation for a given <see cref="StepState"/> in the summary.
  /// </summary>
  /// <param name="state">The <see cref="StepState"/> to configure.</param>
  /// <param name="emoticon">The emoticon to use for the specified state in the summary.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseEmoticonFor(StepState state, string emoticon)
  {
    StepStateEmoticons[state] = emoticon;
    return this;
  }

  /// <summary>
  /// Configures the summary to use the default templates with emoticons for status representation.
  /// </summary>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UsePredefinedTemplate(ScenarioExecutionSummaryPredefinedTemplate predefinedTemplate)
  {
    switch (predefinedTemplate)
    {
      case ScenarioExecutionSummaryPredefinedTemplate.Default:
      {
        TemplateResultSummary = DefaultTemplateResultSummaryEmoticonEnabled;
        TemplateResultException = DefaultTemplateResultException;
        TemplateStepsSummary = DefaultTemplateStepsSummary;
        TemplateStepSummary = DefaultTemplateStepSummaryEmoticonEnabled;
        TemplateStepException = DefaultTemplateStepException;
        TemplateVars = DefaultTemplateVars;
        TemplateVar = DefaultTemplateVar;
        break;
      }

      case ScenarioExecutionSummaryPredefinedTemplate.DefaultWithoutEmoticons:
      {
        TemplateResultSummary = DefaultTemplateResultSummaryEmoticonDisabled;
        TemplateResultException = DefaultTemplateResultException;
        TemplateStepsSummary = DefaultTemplateStepsSummary;
        TemplateStepSummary = DefaultTemplateStepSummaryEmoticonDisabled;
        TemplateStepException = DefaultTemplateStepException;
        TemplateVars = DefaultTemplateVars;
        TemplateVar = DefaultTemplateVar;
        break;
      }

      default:
      {
        throw new ArgumentOutOfRangeException(nameof(predefinedTemplate), predefinedTemplate, null);
      }
    }

    return this;
  }

  /// <summary>
  /// Configures a custom template for rendering the overall scenario result exception details in the summary
  /// using a factory function that takes a placeholder provider as an argument.
  /// </summary>
  /// <param name="factory">
  /// A factory function that takes a <see cref="ScenarioExecutionSummaryTemplatePlaceholderGlobal"/> instance as an argument
  /// and returns a <see cref="TemplateString"/> to use for rendering step exception details.
  /// </param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTemplateForResulException(
    Func<ScenarioExecutionSummaryTemplatePlaceholderGlobal, TemplateString> factory)
  {
    return UseTemplateForResultException(factory(ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance));
  }

  /// <summary>
  /// Configures a custom template for rendering the overall scenario result exception details in the summary.
  /// </summary>
  /// <param name="template">The template to use for rendering the scenario result exception details.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  /// <remarks>
  /// The template can use the following placeholders:
  /// - {{ExceptionMessage}}: The message of the exception.
  /// - {{ExceptionType}}: The type name of the exception.
  /// By default, the template is: "  Exception: {{ExceptionMessage}}".
  /// </remarks>
  public ScenarioExecutionSummaryOptions UseTemplateForResultException(TemplateString template)
  {
    TemplateResultException = template;
    return this;
  }

  /// <summary>
  /// Configures a custom template for rendering the overall scenario result summary.
  /// </summary>
  /// <param name="template">The template to use for rendering the scenario result summary.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  /// <remarks>
  /// The template can use the following placeholders:
  /// - {{ScenarioTitle}}: The title of the scenario.
  /// - {{ScenarioStatusEmoticon}}: The emoticon representing the overall scenario status (e.g., "✅" for Pass, "❌" for Fail).
  /// - {{ScenarioStatusText}}: The overall status of the scenario (e.g., "Pass", "Fail", "Skip").
  /// - {{StepCount}}: The total number of steps in the scenario.
  /// - {{StepPassedCount}}: The number of steps that passed successfully.
  /// - {{StepFailedCount}}: The number of steps that failed.
  /// - {{StepIndecisiveCount}}: The number of steps that are indecisive (e.g., skipped).
  /// By default, the template is: "Scenario \"{{ScenarioTitle}}\" — Status: {{ScenarioStatusEmoticon}} {{ScenarioStatusText}}".
  /// </remarks>
  public ScenarioExecutionSummaryOptions UseTemplateForResultSummary(TemplateString template)
  {
    TemplateResultSummary = template;
    return this;
  }

  /// <summary>
  /// Configures a custom template for rendering the overall scenario result summary
  /// using a factory function that takes a placeholder provider as an argument.
  /// </summary>
  /// <param name="factory">
  /// A factory function that takes a <see cref="ScenarioExecutionSummaryTemplatePlaceholderGlobal"/> instance as an argument
  /// and returns a <see cref="TemplateString"/> to use for rendering the scenario result summary.
  /// </param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTemplateForResultSummary(
    Func<ScenarioExecutionSummaryTemplatePlaceholderGlobal, TemplateString> factory)
  {
    return UseTemplateForResultSummary(factory(ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance));
  }

  /// <summary>
  /// Configures a custom template for rendering step exception details in the summary.
  /// </summary>
  /// <param name="template">The template to use for rendering exception details.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  /// <remarks>
  /// The template can use the following placeholders:
  /// - {{ExceptionMessage}}: The message of the exception.
  /// - {{ExceptionType}}: The type name of the exception.
  /// By default, the template is: "    Exception: {{ExceptionMessage}}".
  /// </remarks>
  public ScenarioExecutionSummaryOptions UseTemplateForStepException(TemplateString template)
  {
    TemplateStepException = template;
    return this;
  }

  /// <summary>
  /// Configures a custom template for rendering step exception details in the summary
  /// using a factory function that takes a placeholder provider as an argument.
  /// </summary>
  /// <param name="factory">
  /// A factory function that takes a <see cref="ScenarioExecutionSummaryTemplatePlaceholderStep"/> instance as an argument
  /// and returns a <see cref="TemplateString"/> to use for rendering step exception details.
  /// </param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTemplateForStepException(
    Func<ScenarioExecutionSummaryTemplatePlaceholderStep, TemplateString> factory)
  {
    return UseTemplateForStepException(factory(ScenarioExecutionSummaryTemplatePlaceholderStep.Instance));
  }

  /// <summary>
  /// Configures a custom template for rendering the steps summary header in the overall scenario summary.
  /// </summary>
  /// <param name="template">The template to use for rendering the steps summary header.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  /// <remarks>
  /// The template can use the following placeholders:
  /// - {{StepCount}}: The total number of steps in the scenario.
  /// - {{StepPassedCount}}: The number of steps that passed successfully.
  /// - {{StepFailedCount}}: The number of steps that failed.
  /// - {{StepIndecisiveCount}}: The number of steps that are indecisive (e.g., skipped).
  /// By default, the template is: "Steps: [{{StepPassedCount}}/{{StepCount}}]".
  /// </remarks>
  public ScenarioExecutionSummaryOptions UseTemplateForStepsSummary(TemplateString template)
  {
    TemplateStepsSummary = template;
    return this;
  }

  /// <summary>
  /// Configures a custom template for rendering the steps summary header in the overall scenario summary
  /// using a factory function that takes a placeholder provider as an argument.
  /// </summary>
  /// <param name="factory">
  /// A factory function that takes a <see cref="ScenarioExecutionSummaryTemplatePlaceholderGlobal"/> instance as an argument
  /// and returns a <see cref="TemplateString"/> to use for rendering the steps summary header.
  /// </param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTemplateForStepsSummary(
    Func<ScenarioExecutionSummaryTemplatePlaceholderGlobal, TemplateString> factory)
  {
    return UseTemplateForStepsSummary(factory(ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance));
  }

  /// <summary>
  /// Configures a custom template for rendering each step summary in the overall scenario summary.
  /// </summary>
  /// <param name="template">The template to use for rendering each step summary.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  /// <remarks>
  /// The template can use the following placeholders:
  /// - {{StepTitle}}: The title of the step.
  /// - {{StepState}}: The state of the step (e.g., "Idle", "Executing", "Done").
  /// - {{StepStatusEmoticon}}: The emoticon representing the step status (e.g., "✅" for Pass, "❌" for Fail).
  /// - {{StepStatusText}}: The status of the step (e.g., "Pass", "Fail", "Skip").
  /// - {{StepNumber}}: The 1-based index of the step in the scenario.
  /// By default, the template is: "  ⁃{{StepNumber}}: [{{StepStatusEmoticon}} {{StepStatusText}}] {{StepTitle}}  (State: {{StepState}})".
  /// </remarks>
  public ScenarioExecutionSummaryOptions UseTemplateForStepSummary(TemplateString template)
  {
    TemplateStepSummary = template;
    return this;
  }

  /// <summary>
  /// Configures a custom template for rendering each step summary in the overall scenario summary
  /// using a factory function that takes a placeholder provider as an argument.
  /// </summary>
  /// <param name="factory">
  /// A factory function that takes a <see cref="ScenarioExecutionSummaryTemplatePlaceholderStep"/> instance as an argument
  /// and returns a <see cref="TemplateString"/> to use for rendering each step summary.
  /// </param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTemplateForStepSummary(
    Func<ScenarioExecutionSummaryTemplatePlaceholderStep, TemplateString> factory)
  {
    return UseTemplateForStepSummary(factory(ScenarioExecutionSummaryTemplatePlaceholderStep.Instance));
  }

  /// <summary>
  /// Configures a custom template for rendering each variable in the summary when variables are included.
  /// </summary>
  /// <param name="template">The template to use for rendering each variable.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  /// <remarks>
  /// The template can use the following placeholders:
  /// - {{VarName}}: The name of the variable.
  /// - {{VarValue}}: The value of the variable.
  /// By default, the template is: "  {{VarName}}: {{VarValue}}".
  /// </remarks>
  public ScenarioExecutionSummaryOptions UseTemplateForVar(TemplateString template)
  {
    TemplateVar = template;
    return this;
  }

  /// <summary>
  /// Configures a custom template for rendering each variable in the summary when variables are included
  /// using a factory function that takes a placeholder provider as an argument.
  /// </summary>
  /// <param name="factory">
  /// A factory function that takes a <see cref="ScenarioExecutionSummaryTemplatePlaceholderVar"/> instance as an argument
  /// and returns a <see cref="TemplateString"/> to use for rendering each variable.
  /// </param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTemplateForVar(
    Func<ScenarioExecutionSummaryTemplatePlaceholderVar, TemplateString> factory)
  {
    return UseTemplateForVar(factory(ScenarioExecutionSummaryTemplatePlaceholderVar.Instance));
  }

  /// <summary>
  /// Configures a custom template for rendering the variables section header in the summary when variables are included.
  /// </summary>
  /// <param name="template">The template to use for rendering the variables section header.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  /// <remarks>
  /// The template can use the following placeholders:
  /// By default, the template is: "Vars:".
  /// </remarks>
  public ScenarioExecutionSummaryOptions UseTemplateForVars(TemplateString template)
  {
    TemplateVars = template;
    return this;
  }

  /// <summary>
  /// Configures a custom template for rendering the variables section header in the summary when variables are included
  /// using a factory function that takes a placeholder provider as an argument.
  /// </summary>
  /// <param name="factory">
  /// A factory function that takes a <see cref="ScenarioExecutionSummaryTemplatePlaceholderGlobal"/> instance as an argument
  /// and returns a <see cref="TemplateString"/> to use for rendering the variables section header.
  /// </param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTemplateForVars(
    Func<ScenarioExecutionSummaryTemplatePlaceholderGlobal, TemplateString> factory)
  {
    return UseTemplateForVars(factory(ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance));
  }

  /// <summary>
  /// Configures the text representation for a given <see cref="ConfirmStatus"/> in the summary.
  /// </summary>
  /// <param name="status">The <see cref="ConfirmStatus"/> to configure.</param>
  /// <param name="text">The text to use for the specified status in the summary.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTextFor(ConfirmStatus status, string text)
  {
    ConfirmStatusTexts[status] = text;
    return this;
  }

  /// <summary>
  /// Configures the text representation for a given <see cref="StepState"/> in the summary.
  /// </summary>
  /// <param name="state">The <see cref="StepState"/> to configure.</param>
  /// <param name="text">The text to use for the specified state in the summary.</param>
  /// <returns>The current <see cref="ScenarioExecutionSummaryOptions"/> instance for chaining.</returns>
  public ScenarioExecutionSummaryOptions UseTextFor(StepState state, string text)
  {
    StepStateTexts[state] = text;
    return this;
  }
}
