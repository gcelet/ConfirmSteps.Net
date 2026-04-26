namespace ConfirmSteps.Net.Tests;

using ConfirmSteps.Reporting;

public class ScenarioExecutionSummaryReporterProvider
{
  public ScenarioExecutionSummaryReporterProvider(string name,
    Action<ScenarioExecutionSummaryOptions>? configure)
  {
    Name = name;
    Configure = configure;
  }

  public Action<ScenarioExecutionSummaryOptions>? Configure { get; }

  public string Name { get; }
}
