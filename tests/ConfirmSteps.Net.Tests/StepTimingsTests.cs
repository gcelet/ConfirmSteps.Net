namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using static CancellationExtensions;

[TestFixture]
public class StepTimingsTests
{
    [Test]
    public async Task StepResultShouldExposeTimingsForEveryPhaseThatRan()
    {
        // Arrange
        Scenario<TimingData> scenario = Scenario.New<TimingData>("[Scenario-Timings-0001]")
            .WithSteps(s => s
                .CodeStep("slow-step",
                    step => step.Execute(_ =>
                    {
                        Thread.Sleep(30);

                        return ConfirmStatus.Success;
                    })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<TimingData> result = await scenario.ConfirmSteps(new TimingData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Success);

        StepResult<TimingData> step = result[0];
        step.Timings.Should().NotBeEmpty();
        step.Timings.Select(t => t.SectionName)
            .Should().Contain(["Prepare", "Execute", "Verify", "Extract"]);
        step.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(25));
        step.Duration.Should().Be(step.Timings.Aggregate(TimeSpan.Zero, (sum, t) => sum + t.Elapsed));
    }

    [Test]
    public async Task StepResultShouldOnlyTimeThePhasesThatActuallyRan()
    {
        // Arrange
        Scenario<TimingData> scenario = Scenario.New<TimingData>("[Scenario-Timings-0002]")
            .WithSteps(s => s
                .CodeStep("failing-step",
                    step => step.Execute(_ => throw new InvalidOperationException("boom"))))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<TimingData> result = await scenario.ConfirmSteps(new TimingData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);

        // A step that fails in Execute never reaches Verify nor Extract, so those phases must not
        // appear: otherwise a caller summing the phases would over-count the step.
        result[0].Timings.Select(t => t.SectionName).Should().Equal("Prepare", "Execute");
    }

    [Test]
    public async Task ConfirmStepResultShouldExposeScenarioDuration()
    {
        // Arrange
        Scenario<TimingData> scenario = Scenario.New<TimingData>("[Scenario-Timings-0003]")
            .WithSteps(s => s
                .CodeStep("step-1", step => step.Execute(_ => ConfirmStatus.Success))
                .WaitStep(20, 30)
                .CodeStep("step-2", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        DateTimeOffset before = DateTimeOffset.UtcNow;
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<TimingData> result = await scenario.ConfirmSteps(new TimingData(), cts.Token);

        // Assert
        result.StartedAt.Should().BeOnOrAfter(before);
        result.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(15));

        // The scenario is at least as long as the sum of its steps.
        TimeSpan stepsTotal = result.StepResults.Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration);
        result.Duration.Should().BeGreaterThanOrEqualTo(stepsTotal - TimeSpan.FromMilliseconds(5));
    }

    public class TimingData
    {
        public long Counter { get; set; }
    }
}
