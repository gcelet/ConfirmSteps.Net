namespace ConfirmSteps.Net.Tests;

using ConfirmSteps.Reporting;

public static class ScenarioExecutionSummaryReporterProviders
{
  public static IEnumerable<ScenarioExecutionSummaryReporterProvider> EnumerateProviders()
  {
    yield return new ScenarioExecutionSummaryReporterProvider("001-Default", null);
    yield return new ScenarioExecutionSummaryReporterProvider("002-DefaultNoEmoticons",
      options => options.UsePredefinedTemplate(ScenarioExecutionSummaryPredefinedTemplate.DefaultWithoutEmoticons)
    );
    yield return new ScenarioExecutionSummaryReporterProvider("003-NoSteps",
      options => options.ExcludePassedSteps().ExcludeSkippedSteps()
    );
    yield return new ScenarioExecutionSummaryReporterProvider("004-CustomEmoticons",
      options => options
        .UseEmoticonFor(ConfirmStatus.Success, "👍")
        .UseEmoticonFor(ConfirmStatus.Failure, "👎")
        .UseEmoticonFor(ConfirmStatus.Indecisive, "🐈‍⬛")
        .UseEmoticonFor(StepState.Idle, "🦥")
        .UseEmoticonFor(StepState.Prepare, "🔪")
        .UseEmoticonFor(StepState.Execute, "🔥")
        .UseEmoticonFor(StepState.Verify, "🔬")
        .UseEmoticonFor(StepState.Extract, "🛸")
        .UseEmoticonFor(StepState.Done, "🙆‍♂️")
    );
  }

  public static IEnumerable<TestFixtureData> EnumerateTestFixtures()
  {
    foreach (ScenarioExecutionSummaryReporterProvider provider in EnumerateProviders())
    {
      yield return new TestFixtureData(provider)
          .SetArgDisplayNames(provider.Name)
        ;
    }
  }
}
