namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using ConfirmSteps.Steps.Http.RequestBuilding;

using static CancellationExtensions;

[TestFixture]
public class ConfirmStepResultTests
{
  [Test]
  public async Task Scenario_And_Step_Should_Have_Title()
  {
    // Arrange
    Scenario<SomeScenarioData> scenario = Scenario.New<SomeScenarioData>("[Scenario]-Every-Step-Should-Have-Title")
      .WithSteps(s => s
        .CodeStep("[Step-001]-Some-Code-Step",
          step => step.Execute(_ => ConfirmStatus.Success)
        )
        .WaitStep("[Step-002]-Some-Wait-Step", TimeSpan.FromMilliseconds(100))
        .WaitStep(TimeSpan.FromMilliseconds(100))
        .HttpStep("[Step-003]-Some-Http-Step", () => RequestBuilder.Get("https://jsonplaceholder.typicode.com/todos/1"))
      )
      .Build()
      ;
    SomeScenarioData data = new();

    // Act
    using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
    ConfirmStepResult<SomeScenarioData> scenarioResult = await scenario.ConfirmSteps(data, cts.Token);

    // Assert
    scenarioResult.Should().NotBeNull();
    scenarioResult.Title.Should().Be("[Scenario]-Every-Step-Should-Have-Title");
    scenarioResult.StepResults.Should().NotBeNull().And.HaveCount(4);
    IReadOnlyList<StepResult<SomeScenarioData>> stepResults = scenarioResult.StepResults;

    stepResults[0].Title.Should().Be("[Step-001]-Some-Code-Step");
    stepResults[1].Title.Should().Be("[Step-002]-Some-Wait-Step");
    stepResults[2].Title.Should().Be("Wait");
    stepResults[3].Title.Should().Be("[Step-003]-Some-Http-Step");
  }

  public class SomeScenarioData
  {
  }
}
