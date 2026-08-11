namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using static CancellationExtensions;

[TestFixture]
public class ScenarioConcurrencyTests
{
    /// <summary>
    /// A built scenario holds no per-run state, so it is meant to be reusable. The one thing that
    /// stood in the way was the shared <see cref="Random"/> backing wait steps, whose instance
    /// methods are not thread-safe — and a wait step sits between every pair of steps.
    /// </summary>
    [Test]
    public async Task ASingleScenarioShouldBeSafeToRunFromSeveralThreadsAtOnce()
    {
        // Arrange
        Scenario<ConcurrencyData> scenario = Scenario.New<ConcurrencyData>("[Scenario-Concurrency-0001]")
            .WithSteps(s => s
                .CodeStep("increment", step => step.Execute(c =>
                {
                    c.ScenarioContext.Data.Counter++;

                    return ConfirmStatus.Success;
                }))
                .WaitStep(1, 3)
                .CodeStep("increment-again", step => step.Execute(c =>
                {
                    c.ScenarioContext.Data.Counter++;

                    return ConfirmStatus.Success;
                }))
                .WaitStep(1, 3))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act — each run gets its own data object, since the scenario mutates it in place.
        ConfirmStepResult<ConcurrencyData>[] results = await Task.WhenAll(
            Enumerable.Range(0, 32).Select(_ =>
                Task.Run(() => scenario.ConfirmSteps(new ConcurrencyData(), cts.Token), cts.Token)));

        // Assert
        results.Should().AllSatisfy(r => r.Status.Should().Be(ConfirmStatus.Success));
        results.Should().AllSatisfy(r => r.Data.Counter.Should().Be(2));
    }

    public class ConcurrencyData
    {
        public long Counter { get; set; }
    }
}
