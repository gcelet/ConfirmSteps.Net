namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using ConfirmSteps.Steps;

using Microsoft.Extensions.DependencyInjection;

using static CancellationExtensions;

[TestFixture]
public class StepFailurePolicyTests
{
    /// <summary>
    /// The historical behaviour, and the one that applies when nothing is registered.
    /// </summary>
    [Test]
    public async Task WithoutAPolicyAScenarioShouldSkipWhatFollowsAFailure()
    {
        // Arrange
        List<string> executed = new();

        using Scenario<PolicyData> scenario = Scenario.New<PolicyData>("[Scenario-Policy-0001]")
            .WithSteps(s => s
                .CodeStep("first", step => step.Execute(_ => Record(executed, "first", ConfirmStatus.Failure)))
                .CodeStep("second", step => step.Execute(_ => Record(executed, "second", ConfirmStatus.Success))))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<PolicyData> result = await scenario.ConfirmSteps(new PolicyData(), cts.Token);

        // Assert
        executed.Should().Equal("first");
        result.Status.Should().Be(ConfirmStatus.Failure);
        result.StepResults[1].Status.Should().Be(ConfirmStatus.Indecisive);
        result.StepResults[1].State.Should().Be(StepState.Idle);
    }

    [Test]
    public async Task APolicyMayLetTheRemainingStepsRunForObservation()
    {
        // Arrange
        List<string> executed = new();
        RecordingPolicy policy = new(StepFailureAction.ContinueForObservation);

        using Scenario<PolicyData> scenario = Scenario.New<PolicyData>("[Scenario-Policy-0002]")
            .WithServices(services => services.AddSingleton<IStepFailurePolicy<PolicyData>>(policy))
            .WithSteps(s => s
                .CodeStep("first", step => step.Execute(_ => Record(executed, "first", ConfirmStatus.Failure)))
                .CodeStep("second", step => step.Execute(_ => Record(executed, "second", ConfirmStatus.Success)))
                .CodeStep("third", step => step.Execute(_ => Record(executed, "third", ConfirmStatus.Success))))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<PolicyData> result = await scenario.ConfirmSteps(new PolicyData(), cts.Token);

        // Assert
        executed.Should().Equal("first", "second", "third");
        result.StepResults[1].Status.Should().Be(ConfirmStatus.Success);
        result.StepResults[2].Status.Should().Be(ConfirmStatus.Success);
        policy.Consultations.Should().Equal("0:first:Failure");
    }

    /// <summary>
    /// The invariant the extension point must not be able to break: a step that fails fails the
    /// scenario, whatever runs afterwards and however well it goes.
    /// </summary>
    [Test]
    public async Task AStepThatSucceedsAfterAFailureShouldNotRescueTheScenario()
    {
        // Arrange
        using Scenario<PolicyData> scenario = Scenario.New<PolicyData>("[Scenario-Policy-0003]")
            .WithServices(services => services.AddSingleton<IStepFailurePolicy<PolicyData>>(
                new RecordingPolicy(StepFailureAction.ContinueForObservation)))
            .WithSteps(s => s
                .CodeStep("failing", step => step.Execute(_ => ConfirmStatus.Failure))
                .CodeStep("succeeding", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<PolicyData> result = await scenario.ConfirmSteps(new PolicyData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);
    }

    /// <summary>
    /// Asked once, about the step that decided the outcome — not again about later failures, since
    /// the verdict is already settled.
    /// </summary>
    [Test]
    public async Task APolicyShouldBeConsultedOnceAndOnlyOnANonSuccess()
    {
        // Arrange
        RecordingPolicy policy = new(StepFailureAction.ContinueForObservation);

        using Scenario<PolicyData> scenario = Scenario.New<PolicyData>("[Scenario-Policy-0004]")
            .WithServices(services => services.AddSingleton<IStepFailurePolicy<PolicyData>>(policy))
            .WithSteps(s => s
                .CodeStep("ok", step => step.Execute(_ => ConfirmStatus.Success))
                .CodeStep("first-failure", step => step.Execute(_ => ConfirmStatus.Failure))
                .CodeStep("second-failure", step => step.Execute(_ => ConfirmStatus.Failure)))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new PolicyData(), cts.Token);

        // Assert
        policy.Consultations.Should().Equal("1:first-failure:Failure");
    }

    /// <summary>
    /// Returning the default explicitly must behave exactly like registering nothing.
    /// </summary>
    [Test]
    public async Task APolicyReturningSkipShouldBehaveLikeNoPolicyAtAll()
    {
        // Arrange
        List<string> executed = new();

        using Scenario<PolicyData> scenario = Scenario.New<PolicyData>("[Scenario-Policy-0005]")
            .WithServices(services => services.AddSingleton<IStepFailurePolicy<PolicyData>>(
                new RecordingPolicy(StepFailureAction.SkipRemainingSteps)))
            .WithSteps(s => s
                .CodeStep("first", step => step.Execute(_ => Record(executed, "first", ConfirmStatus.Failure)))
                .CodeStep("second", step => step.Execute(_ => Record(executed, "second", ConfirmStatus.Success))))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<PolicyData> result = await scenario.ConfirmSteps(new PolicyData(), cts.Token);

        // Assert
        executed.Should().Equal("first");
        result.Status.Should().Be(ConfirmStatus.Failure);
    }

    private static ConfirmStatus Record(List<string> executed, string name, ConfirmStatus status)
    {
        executed.Add(name);

        return status;
    }

    private sealed class PolicyData
    {
    }

    private sealed class RecordingPolicy : IStepFailurePolicy<PolicyData>
    {
        private readonly StepFailureAction action;

        public RecordingPolicy(StepFailureAction action)
        {
            this.action = action;
        }

        public List<string> Consultations { get; } = new();

        public StepFailureAction OnStepFailed(
            ScenarioContext<PolicyData> scenarioContext, StepResult<PolicyData> stepResult, int stepIndex)
        {
            Consultations.Add($"{stepIndex}:{stepResult.Title}:{stepResult.Status}");

            return action;
        }
    }
}
