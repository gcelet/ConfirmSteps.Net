namespace ConfirmSteps;

using System.Diagnostics;

using ConfirmSteps.Data;
using ConfirmSteps.Steps;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides a static entry point to create new scenarios.
/// </summary>
public static class Scenario
{
  /// <summary>
  /// Starts the creation of a new scenario.
  /// </summary>
  /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
  /// <param name="title">The title of the scenario.</param>
  /// <returns>An <see cref="IScenarioCustomizer{T}"/> to configure the scenario.</returns>
  public static IScenarioCustomizer<T> New<T>(string title)
    where T : class
  {
    return new ScenarioBuilder<T>(title);
  }
}

/// <summary>
/// Represents a scenario consisting of multiple steps to be executed.
/// </summary>
/// <typeparam name="T">The type of the data object being processed.</typeparam>
public sealed class Scenario<T> : IAsyncDisposable, IDisposable
  where T : class
{
  private readonly ServiceProviderOwnership ownership;
  private bool disposed;

  /// <summary>
  /// Initializes a new instance of the <see cref="Scenario{T}"/> class over a service provider the
  /// caller owns and disposes.
  /// </summary>
  /// <param name="title">The title of the scenario.</param>
  /// <param name="steps">The list of steps to execute.</param>
  /// <param name="services">The service provider for dependency injection.</param>
  public Scenario(string title, IReadOnlyList<IStep<T>> steps, IServiceProvider services)
    : this(title, steps, services, ServiceProviderOwnership.External)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Scenario{T}"/> class.
  /// </summary>
  /// <param name="title">The title of the scenario.</param>
  /// <param name="steps">The list of steps to execute.</param>
  /// <param name="services">The service provider for dependency injection.</param>
  /// <param name="ownership">
  /// Who is responsible for disposing <paramref name="services"/>. Only the builder passes
  /// <see cref="ServiceProviderOwnership.Scenario"/>: a caller supplying their own container keeps
  /// control of its lifetime, which is what makes this addition behaviourally safe.
  /// </param>
  public Scenario(string title, IReadOnlyList<IStep<T>> steps, IServiceProvider services,
    ServiceProviderOwnership ownership)
  {
    Title = title;
    Steps = steps;
    Services = services;
    this.ownership = ownership;
  }

  /// <summary>
  /// Gets the title of the scenario.
  /// </summary>
  public string Title { get; }

  /// <summary>
  /// Gets the service provider backing the scenario.
  /// </summary>
  /// <remarks>
  /// Exposed so a host can inspect or reuse the container it configured through
  /// <see cref="IScenarioCustomizer{T}.WithServices"/>.
  /// </remarks>
  public IServiceProvider Services { get; }

  private IReadOnlyList<IStep<T>> Steps { get; }

  /// <summary>
  /// Executes the scenario with the provided data.
  /// </summary>
  /// <param name="data">The initial data object.</param>
  /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
  /// <returns>A <see cref="ConfirmStepResult{T}"/> containing the results of the execution.</returns>
  public async Task<ConfirmStepResult<T>> ConfirmSteps(T data, CancellationToken cancellationToken)
  {
    DateTimeOffset startedAt = DateTimeOffset.UtcNow;
    long startTimestamp = Stopwatch.GetTimestamp();
    int nbSteps = Steps.Count;
    VarManager<T> varManager = Services.GetRequiredService<VarManager<T>>();
    IReadOnlyDictionary<string, object> globalVars = varManager.Extract(data);
    IServiceScopeFactory serviceScopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
    ScenarioContext<T> scenarioContext = new(data, Services)
    {
      Vars = new Dictionary<string, object>(globalVars, StringComparer.Ordinal)
    };

    Exception? scenarioException = null;
    List<StepResult<T>> stepResults = new();
    ConfirmStatus scenarioStatus = ConfirmStatus.Success;

    // Empty when nothing is registered, so a scenario without observers pays a single lookup.
    IReadOnlyList<IScenarioObserver<T>> observers = Services.GetServices<IScenarioObserver<T>>().ToList();

    await Notify(observers, o => o.OnScenarioStarting(scenarioContext, nbSteps, cancellationToken))
      .ConfigureAwait(false);

    for (int i = 0; i < nbSteps; i++)
    {
      IStep<T> currentStep = Steps[i];
      StepResult<T> stepResult;

      await Notify(observers, o => o.OnStepStarting(scenarioContext, currentStep, i, cancellationToken))
        .ConfigureAwait(false);

      if (scenarioStatus == ConfirmStatus.Success)
      {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        StepContext<T> stepContext = new(scenarioContext, serviceProvider, scenarioContext.Vars);

        stepResult =
          await currentStep.ConfirmStep(stepContext, cancellationToken).ConfigureAwait(false);

        scenarioContext.Vars = stepResult.Vars.Concat(scenarioContext.Vars)
          .GroupBy(kvp => kvp.Key, StringComparer.Ordinal)
          .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.Ordinal);

        scenarioStatus = stepResult.Status;
        scenarioException = stepResult.Exception;
      }
      else
      {
        stepResult = new StepResult<T>
        {
          Title = currentStep.Title,
          Status = ConfirmStatus.Indecisive,
          State = StepState.Idle,
          Vars = new Dictionary<string, object>(scenarioContext.Vars, StringComparer.Ordinal)
        };
      }

      await Notify(observers, o => o.OnStepCompleted(scenarioContext, stepResult, i, cancellationToken))
        .ConfigureAwait(false);

      stepResults.Add(stepResult);
    }

    ConfirmStepResult<T> result = new(Title, scenarioStatus, stepResults, scenarioContext.Data,
      new Dictionary<string, object>(scenarioContext.Vars, StringComparer.Ordinal), scenarioException)
    {
      StartedAt = startedAt,
      Duration = GetElapsedTime(startTimestamp),
    };

    await Notify(observers, o => o.OnScenarioCompleted(result, cancellationToken)).ConfigureAwait(false);

    return result;
  }

  /// <summary>
  /// Disposes the service provider the scenario owns.
  /// </summary>
  /// <remarks>
  /// <see cref="ScenarioBuilder{T}"/> calls <c>BuildServiceProvider()</c>, and until now nothing
  /// ever disposed the result. A process building many scenarios therefore accumulated containers
  /// and every singleton they held — including the default <c>HttpClient</c> and its connection
  /// pool. A scenario built over a caller-supplied provider disposes nothing.
  /// </remarks>
  /// <returns>A task that completes once the owned provider is disposed.</returns>
  public async ValueTask DisposeAsync()
  {
    if (disposed)
    {
      return;
    }

    disposed = true;

    if (ownership != ServiceProviderOwnership.Scenario)
    {
      return;
    }

    switch (Services)
    {
      case IAsyncDisposable asyncDisposable:
      {
        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        break;
      }

      case IDisposable disposable:
      {
        disposable.Dispose();
        break;
      }
    }
  }

  /// <inheritdoc cref="DisposeAsync" />
  public void Dispose()
  {
    if (disposed)
    {
      return;
    }

    disposed = true;

    if (ownership == ServiceProviderOwnership.Scenario && Services is IDisposable disposable)
    {
      disposable.Dispose();
    }
  }

  /// <summary>
  /// Invokes every observer, swallowing whatever they throw.
  /// </summary>
  /// <remarks>
  /// Observing must not change the outcome being observed. A dashboard that fails to paint a line
  /// has no business turning a passing step into a failing one, and a caller reading the result
  /// would have no way to tell the two apart.
  /// </remarks>
  private static async ValueTask Notify(IReadOnlyList<IScenarioObserver<T>> observers,
    Func<IScenarioObserver<T>, ValueTask> notification)
  {
    for (int i = 0; i < observers.Count; i++)
    {
      try
      {
        await notification(observers[i]).ConfigureAwait(false);
      }
      catch (Exception exception)
      {
        // Reported to the debugger rather than rethrown: loud enough to be noticed while writing
        // an observer, silent enough not to alter the run in production.
        Debug.WriteLine($"Scenario observer threw and was ignored: {exception}");
      }
    }
  }

  private static TimeSpan GetElapsedTime(long startTimestamp)
  {
#if NET7_0_OR_GREATER
    return Stopwatch.GetElapsedTime(startTimestamp);
#else
    long elapsed = Stopwatch.GetTimestamp() - startTimestamp;

    return TimeSpan.FromTicks((long)(elapsed * (TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency)));
#endif
  }
}
