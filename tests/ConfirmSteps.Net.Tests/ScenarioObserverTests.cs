namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using ConfirmSteps.Steps;

using Microsoft.Extensions.DependencyInjection;

using static CancellationExtensions;

[TestFixture]
public class ScenarioObserverTests
{
    [Test]
    public async Task AnObserverShouldSeeEveryStepInOrder()
    {
        // Arrange
        RecordingObserver observer = new();

        using Scenario<ObserverData> scenario = Scenario.New<ObserverData>("[Scenario-Observer-0001]")
            .WithServices(services => services.AddSingleton<IScenarioObserver<ObserverData>>(observer))
            .WithSteps(s => s
                .CodeStep("first", step => step.Execute(_ => ConfirmStatus.Success))
                .CodeStep("second", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new ObserverData(), cts.Token);

        // Assert
        observer.Events.Should().Equal(
            "scenario-starting:2",
            "step-starting:0:first",
            "step-completed:0:first:Success",
            "step-starting:1:second",
            "step-completed:1:second:Success",
            "scenario-completed:Success");
    }

    /// <summary>
    /// Steps skipped after a failure are reported too: a progress display that stopped emitting
    /// would leave the reader unable to tell a skipped step from one still running.
    /// </summary>
    [Test]
    public async Task AnObserverShouldAlsoSeeStepsSkippedAfterAFailure()
    {
        // Arrange
        RecordingObserver observer = new();

        using Scenario<ObserverData> scenario = Scenario.New<ObserverData>("[Scenario-Observer-0002]")
            .WithServices(services => services.AddSingleton<IScenarioObserver<ObserverData>>(observer))
            .WithSteps(s => s
                .CodeStep("boom", step => step.Execute(_ => ConfirmStatus.Failure))
                .CodeStep("never", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new ObserverData(), cts.Token);

        // Assert
        observer.Events.Should().Contain("step-completed:1:never:Indecisive");
    }

    /// <summary>
    /// Observing must not change the outcome being observed.
    /// </summary>
    [Test]
    public async Task AnObserverThatThrowsShouldNotFailTheScenario()
    {
        // Arrange
        using Scenario<ObserverData> scenario = Scenario.New<ObserverData>("[Scenario-Observer-0003]")
            .WithServices(services =>
                services.AddSingleton<IScenarioObserver<ObserverData>>(new ThrowingObserver()))
            .WithSteps(s => s.CodeStep("fine", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<ObserverData> result = await scenario.ConfirmSteps(new ObserverData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Success);
    }

    [Test]
    public async Task AScenarioWithoutObserversShouldBehaveExactlyAsBefore()
    {
        // Arrange
        using Scenario<ObserverData> scenario = Scenario.New<ObserverData>("[Scenario-Observer-0004]")
            .WithSteps(s => s.CodeStep("fine", step => step.Execute(c =>
            {
                c.ScenarioContext.Data.Counter++;

                return ConfirmStatus.Success;
            })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<ObserverData> result = await scenario.ConfirmSteps(new ObserverData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Success);
        result.Data.Counter.Should().Be(1);
    }

    public class ObserverData
    {
        public long Counter { get; set; }
    }

    private sealed class RecordingObserver : ScenarioObserver<ObserverData>
    {
        public List<string> Events { get; } = new();

        public override ValueTask OnScenarioStarting(ScenarioContext<ObserverData> scenarioContext,
            int stepCount, CancellationToken cancellationToken)
        {
            Events.Add($"scenario-starting:{stepCount}");

            return default;
        }

        public override ValueTask OnStepStarting(ScenarioContext<ObserverData> scenarioContext,
            IStep<ObserverData> step, int stepIndex, CancellationToken cancellationToken)
        {
            Events.Add($"step-starting:{stepIndex}:{step.Title}");

            return default;
        }

        public override ValueTask OnStepCompleted(ScenarioContext<ObserverData> scenarioContext,
            StepResult<ObserverData> stepResult, int stepIndex, CancellationToken cancellationToken)
        {
            Events.Add($"step-completed:{stepIndex}:{stepResult.Title}:{stepResult.Status}");

            return default;
        }

        public override ValueTask OnScenarioCompleted(ConfirmStepResult<ObserverData> confirmStepResult,
            CancellationToken cancellationToken)
        {
            Events.Add($"scenario-completed:{confirmStepResult.Status}");

            return default;
        }
    }

    private sealed class ThrowingObserver : ScenarioObserver<ObserverData>
    {
        public override ValueTask OnStepCompleted(ScenarioContext<ObserverData> scenarioContext,
            StepResult<ObserverData> stepResult, int stepIndex, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("observer is broken");
        }
    }
}
