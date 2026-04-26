namespace ConfirmSteps.Reporting;

using System.Text;

/// <summary>
/// The default <see cref="IScenarioExecutionSummaryReporter"/> that produces a plain-text summary.
/// </summary>
public sealed class ScenarioExecutionSummaryReporter : IScenarioExecutionSummaryReporter
{
  /// <summary>
  /// Initializes a new instance of <see cref="ScenarioExecutionSummaryReporter"/> with optional configuration.
  /// </summary>
  /// <param name="configure">Optional delegate to configure the reporter options.</param>
  public ScenarioExecutionSummaryReporter(Action<ScenarioExecutionSummaryOptions>? configure = null)
  {
    Options = new ScenarioExecutionSummaryOptions();
    configure?.Invoke(Options);
  }

  /// <summary>
  /// Gets the default reporter instance with no customization.
  /// </summary>
  public static ScenarioExecutionSummaryReporter Default { get; } = new();

  private static void AppendExceptionTemplateValues(Dictionary<string, object> templateValues,
    Exception? exception)
  {
    if (exception is null)
    {
      templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.ExceptionMessage.Name] = string.Empty;
      templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.ExceptionType.Name] = string.Empty;
    }
    else
    {
      templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.ExceptionMessage.Name] = exception.Message;
      templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.ExceptionType.Name] = exception.GetType().Name;
    }
  }

  private ScenarioExecutionSummaryOptions Options { get; }

  /// <inheritdoc />
  public string Report<T>(ConfirmStepResult<T> result) where T : class
  {
    StringBuilder sb = new();
    Dictionary<string, object> templateValues = new(StringComparer.Ordinal);

    AppendTemplateValues(templateValues, result);

    if (Options.ShouldIncludeResultSummary)
    {
      sb.AppendLine(Options.TemplateResultSummary.Render(templateValues));

      if (Options.ShouldIncludeResultException && result.Exception is not null)
      {
        sb.AppendLine(Options.TemplateResultException.Render(templateValues));
      }
    }

    if (Options.ShouldIncludeStepsSummary)
    {
      sb.AppendLine(Options.TemplateStepsSummary.Render(templateValues));
    }

    if (Options.ShouldIncludeSteps)
    {
      for (int index = 0; index < result.StepResults.Count; index++)
      {
        StepResult<T> step = result.StepResults[index];
        if (step.Status == ConfirmStatus.Success && !Options.ShouldIncludePassedSteps)
        {
          continue;
        }

        if (step.Status == ConfirmStatus.Indecisive && !Options.ShouldIncludeSkippedSteps)
        {
          continue;
        }

        Dictionary<string, object> stepTemplateValues = new(templateValues, StringComparer.Ordinal);

        AppendTemplateValues(stepTemplateValues, step, index, result.StepResults.Count);

        sb.AppendLine(Options.TemplateStepSummary.Render(stepTemplateValues));

        if (Options.ShouldIncludeStepException && step.Exception is not null)
        {
          sb.AppendLine(Options.TemplateStepException.Render(stepTemplateValues));
        }
      }
    }

    if (Options.ShouldIncludeVars && result.Vars.Count > 0)
    {
      Dictionary<string, object> varsTemplateValues = new(templateValues, StringComparer.Ordinal);

      sb.AppendLine();
      sb.AppendLine(Options.TemplateVars.Render(varsTemplateValues));

      foreach ((string key, object value) in result.Vars)
      {
        varsTemplateValues[ScenarioExecutionSummaryTemplatePlaceholderVar.Instance.VarName.Name] = key;
        varsTemplateValues[ScenarioExecutionSummaryTemplatePlaceholderVar.Instance.VarValue.Name] = value;

        sb.AppendLine(Options.TemplateVar.Render(varsTemplateValues));
      }
    }

    return sb.ToString().TrimEnd();
  }

  private void AppendTemplateValues<T>(Dictionary<string, object> templateValues, StepResult<T> step,
    int stepIndex, int stepCount)
    where T : class
  {
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderStep.Instance.StepTitle.Name] = step.Title;
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderStep.Instance.StepStateEmoticon.Name] =
      Options.StepStateEmoticons.TryGetValue(step.State, out string? stateEmoStr)
        ? stateEmoStr
        : string.Empty;
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderStep.Instance.StepStateText.Name] =
      Options.StepStateTexts.TryGetValue(step.State, out string? stateStr)
        ? stateStr
        : step.State.ToString();
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderStep.Instance.StepStatusEmoticon.Name] =
      Options.ConfirmStatusEmoticons.TryGetValue(step.Status, out string? statusEmoStr)
        ? statusEmoStr
        : string.Empty;
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderStep.Instance.StepStatusText.Name] =
      Options.ConfirmStatusTexts.TryGetValue(step.Status, out string? statusStr)
        ? statusStr
        : step.Status.ToString();

    int paddingWidth = stepCount.ToString().Length;
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderStep.Instance.StepNumber.Name] =
      (stepIndex + 1).ToString($"D{paddingWidth}");

    AppendExceptionTemplateValues(templateValues, step.Exception);
  }

  private void AppendTemplateValues<T>(Dictionary<string, object> templateValues, ConfirmStepResult<T> result)
    where T : class
  {
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.ScenarioTitle.Name] = result.Title;
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.ScenarioStatusEmoticon.Name] =
      Options.ConfirmStatusEmoticons.TryGetValue(result.Status, out string? statusEmoStr)
        ? statusEmoStr
        : string.Empty;
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.ScenarioStatusText.Name] =
      Options.ConfirmStatusTexts.TryGetValue(result.Status, out string? statusStr)
        ? statusStr
        : result.Status.ToString();
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.StepCount.Name] = result.StepResults.Count;

    int passedSteps = 0;
    int failedSteps = 0;
    int indecisiveSteps = 0;

    foreach (StepResult<T> step in result.StepResults)
    {
      switch (step.Status)
      {
        case ConfirmStatus.Success:
        {
          passedSteps++;
          break;
        }

        case ConfirmStatus.Failure:
        {
          failedSteps++;
          break;
        }

        case ConfirmStatus.Indecisive:
        {
          indecisiveSteps++;
          break;
        }
      }
    }

    templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.StepPassedCount.Name] = passedSteps;
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.StepFailedCount.Name] = failedSteps;
    templateValues[ScenarioExecutionSummaryTemplatePlaceholderGlobal.Instance.StepIndecisiveCount.Name] = indecisiveSteps;
    AppendExceptionTemplateValues(templateValues, result.Exception);
  }
}
