namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using ConfirmSteps.Steps;

using Microsoft.Extensions.DependencyInjection;

[TestFixture]
public class ScenarioCancellationTests
{
    /// <summary>
    /// The distinction that decides whether a reported error rate is real: a host stopping its own
    /// run must not see that stop counted as the system under test breaking.
    /// </summary>
    [Test]
    public async Task ACancelledStepShouldBeIndecisiveRatherThanFailed()
    {
        // Arrange
        using CancellationTokenSource cts = new();

        using Scenario<CancellationData> scenario = Scenario.New<CancellationData>("[Scenario-Cancel-0001]")
            .WithSteps(s => s
                .CodeStep("cancels", step => step.Execute(_ =>
                {
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();

                    return ConfirmStatus.Success;
                }))
                .CodeStep("never", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        // Act
        ConfirmStepResult<CancellationData> result =
            await scenario.ConfirmSteps(new CancellationData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Indecisive);
        result.WasCancelled.Should().BeTrue();

        result[0].Status.Should().Be(ConfirmStatus.Indecisive);
        result[0].WasCancelled.Should().BeTrue();
        result[0].Exception.Should().BeNull("a cancellation the caller asked for is not an error");

        // The phase reached is preserved, so a reader can still see how far the step got.
        result[0].State.Should().Be(StepState.Done);
    }

    [Test]
    public async Task AGenuineFailureShouldStillBeAFailure()
    {
        // Arrange
        using CancellationTokenSource cts = new();

        using Scenario<CancellationData> scenario = Scenario.New<CancellationData>("[Scenario-Cancel-0002]")
            .WithSteps(s => s
                .CodeStep("boom", step => step.Execute(_ => throw new InvalidOperationException("boom"))))
            .Build();

        // Act
        ConfirmStepResult<CancellationData> result =
            await scenario.ConfirmSteps(new CancellationData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);
        result.WasCancelled.Should().BeFalse();
        result[0].Exception.Should().BeOfType<InvalidOperationException>();
    }

    /// <summary>
    /// An <see cref="OperationCanceledException"/> raised while the token is NOT signalled is a
    /// genuine fault, not a cooperative stop, and must keep being reported as one.
    /// </summary>
    [Test]
    public async Task AnUnrelatedCancellationExceptionShouldStillBeAFailure()
    {
        // Arrange
        using CancellationTokenSource cts = new();

        using Scenario<CancellationData> scenario = Scenario.New<CancellationData>("[Scenario-Cancel-0003]")
            .WithSteps(s => s
                .CodeStep("odd", step => step.Execute(_ => throw new OperationCanceledException("unrelated"))))
            .Build();

        // Act
        ConfirmStepResult<CancellationData> result =
            await scenario.ConfirmSteps(new CancellationData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);
        result[0].WasCancelled.Should().BeFalse();
        result[0].Exception.Should().BeOfType<OperationCanceledException>();
    }

    /// <summary>
    /// Cancellation ends the run even when the host asked to carry on past a failure: continuing
    /// would only produce more cancelled steps, which teach nobody anything.
    /// </summary>
    [Test]
    public async Task CancellationShouldEndTheRunEvenWhenThePolicyAsksToContinue()
    {
        // Arrange
        int ran = 0;
        using CancellationTokenSource cts = new();

        using Scenario<CancellationData> scenario = Scenario.New<CancellationData>("[Scenario-Cancel-0004]")
            .WithServices(services => services.AddSingleton<IStepFailurePolicy<CancellationData>>(
                new AlwaysContinuePolicy()))
            .WithSteps(s => s
                .CodeStep("cancels", step => step.Execute(_ =>
                {
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();

                    return ConfirmStatus.Success;
                }))
                .CodeStep("after", step => step.Execute(_ =>
                {
                    ran++;

                    return ConfirmStatus.Success;
                })))
            .Build();

        // Act
        ConfirmStepResult<CancellationData> result =
            await scenario.ConfirmSteps(new CancellationData(), cts.Token);

        // Assert
        ran.Should().Be(0);
        result.WasCancelled.Should().BeTrue();
    }

    public class CancellationData
    {
        public long Counter { get; set; }
    }

    private sealed class AlwaysContinuePolicy : IStepFailurePolicy<CancellationData>
    {
        public StepFailureAction OnStepFailed(
            ScenarioContext<CancellationData> scenarioContext,
            StepResult<CancellationData> stepResult,
            int stepIndex)
            => StepFailureAction.ContinueForObservation;
    }
}
