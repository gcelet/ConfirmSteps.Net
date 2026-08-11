namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using Microsoft.Extensions.DependencyInjection;

[TestFixture]
public class ScenarioDisposalTests
{
    [Test]
    public async Task DisposingABuiltScenarioShouldDisposeTheSingletonsItsContainerCreated()
    {
        // Arrange
        Scenario<DisposalData> scenario = Scenario.New<DisposalData>("[Scenario-Disposal-0001]")
            .WithServices(services => services.AddSingleton<TrackedService>())
            .WithSteps(s => s.CodeStep("noop", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        TrackedService tracked = scenario.Services.GetRequiredService<TrackedService>();

        // Act
        await scenario.DisposeAsync();

        // Assert
        tracked.Disposed.Should().BeTrue();
    }

    /// <summary>
    /// Microsoft's container never disposes instances it did not create, so handing it a
    /// ready-made object keeps that object the caller's responsibility. Worth pinning down: it is
    /// the difference between the scenario releasing an <c>HttpClient</c> it built and one you
    /// supplied.
    /// </summary>
    [Test]
    public async Task DisposingAScenarioShouldNotDisposeInstancesTheCallerSupplied()
    {
        // Arrange
        TrackedService tracked = new();

        Scenario<DisposalData> scenario = Scenario.New<DisposalData>("[Scenario-Disposal-0002]")
            .WithServices(services => services.AddSingleton(tracked))
            .WithSteps(s => s.CodeStep("noop", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        // Act
        await scenario.DisposeAsync();

        // Assert
        tracked.Disposed.Should().BeFalse();
    }

    [Test]
    public async Task DisposingTwiceShouldBeHarmless()
    {
        // Arrange
        Scenario<DisposalData> scenario = Scenario.New<DisposalData>("[Scenario-Disposal-0003]")
            .WithServices(services => services.AddSingleton<TrackedService>())
            .WithSteps(s => s.CodeStep("noop", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        TrackedService tracked = scenario.Services.GetRequiredService<TrackedService>();

        // Act
        await scenario.DisposeAsync();
        await scenario.DisposeAsync();
        scenario.Dispose();

        // Assert
        tracked.DisposeCount.Should().Be(1);
    }

    /// <summary>
    /// A caller supplying their own container keeps control of its lifetime: this is what makes
    /// adding disposal safe for existing code.
    /// </summary>
    [Test]
    public async Task DisposingAScenarioOverACallerOwnedProviderShouldDisposeNothing()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<TrackedService>();

        await using ServiceProvider provider = services.BuildServiceProvider();
        TrackedService tracked = provider.GetRequiredService<TrackedService>();
        Scenario<DisposalData> scenario = new("[Scenario-Disposal-0004]", [], provider);

        // Act
        await scenario.DisposeAsync();

        // Assert
        tracked.Disposed.Should().BeFalse();
    }

    [Test]
    public void ScenarioServicesShouldBeReachable()
    {
        // Arrange
        TrackedService tracked = new();

        using Scenario<DisposalData> scenario = Scenario.New<DisposalData>("[Scenario-Disposal-0005]")
            .WithServices(services => services.AddSingleton(tracked))
            .WithSteps(s => s.CodeStep("noop", step => step.Execute(_ => ConfirmStatus.Success)))
            .Build();

        // Assert
        scenario.Services.GetService<TrackedService>().Should().BeSameAs(tracked);
    }

    public class DisposalData
    {
        public long Counter { get; set; }
    }

    private sealed class TrackedService : IDisposable
    {
        public int DisposeCount { get; private set; }

        public bool Disposed => DisposeCount > 0;

        public void Dispose() => DisposeCount++;
    }
}
