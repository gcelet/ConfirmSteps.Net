namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using static CancellationExtensions;

[TestFixture]
public class StepItemDisposalTests
{
    [Test]
    public async Task StepItemsShouldSurviveByDefault()
    {
        // Arrange
        TrackedItem item = new();
        using Scenario<DisposalData> scenario = BuildScenario(item);
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new DisposalData(), cts.Token);

        // Assert — the historical behaviour, kept so existing callers reading a response after the
        // step are unaffected.
        item.Disposed.Should().BeFalse();
    }

    [Test]
    public async Task TurningOnDisposalShouldReleaseStepItems()
    {
        // Arrange
        TrackedItem item = new();
        using Scenario<DisposalData> scenario = BuildScenario(item);
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        ScenarioRunOptions options = ScenarioRunOptions.Default with { DisposeStepItems = true };

        // Act
        ConfirmStepResult<DisposalData> result =
            await scenario.ConfirmSteps(new DisposalData(), options, cts.Token);

        // Assert
        item.Disposed.Should().BeTrue();

        // Nothing the result exposes depends on the items surviving: vars are copied out by the step.
        result.Status.Should().Be(ConfirmStatus.Success);
        result.Vars.Should().ContainKey("marker");
    }

    private static Scenario<DisposalData> BuildScenario(TrackedItem item)
        => Scenario.New<DisposalData>("[Scenario-ItemDisposal-0001]")
            .WithSteps(s => s
                .CodeStep("parks-an-item", step => step.Execute(c =>
                {
                    c.AddItem(item);
                    c.Vars["marker"] = "kept";

                    return ConfirmStatus.Success;
                })))
            .Build();

    public class DisposalData
    {
        public long Counter { get; set; }
    }

    private sealed class TrackedItem : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }
}
