namespace ConfirmSteps;

using ConfirmSteps.Steps;

/// <summary>
/// Represents the result of a single step execution.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class StepResult<T>
  where T : class
{
  /// <summary>
  /// Gets the total time spent in the step, across all of its phases.
  /// </summary>
  public TimeSpan Duration
  {
    get
    {
      TimeSpan total = TimeSpan.Zero;

      for (int i = 0; i < Timings.Count; i++)
      {
        total += Timings[i].Elapsed;
      }

      return total;
    }
  }

  /// <summary>
  /// Gets or sets the time spent in each phase of the step: Prepare, Execute, Verify and Extract.
  /// </summary>
  /// <remarks>
  /// Only the phases that actually ran are present: a step that fails during Execute has no Verify
  /// nor Extract entry.
  /// </remarks>
  public IReadOnlyList<StepProfiler.StepSectionStat> Timings { get; set; } = [];

  /// <summary>
  /// Gets or sets the exception that occurred during step execution, if any.
  /// </summary>
  public Exception? Exception { get; set; }

  /// <summary>
  /// Gets or sets the state of the step.
  /// </summary>
  public StepState State { get; set; } = StepState.Idle;

  /// <summary>
  /// Gets or sets the status of the step.
  /// </summary>
  public ConfirmStatus Status { get; set; } = ConfirmStatus.Indecisive;

  /// <summary>
  /// Gets or sets the title of the step.
  /// </summary>
  public required string Title { get; init; }

  /// <summary>
  /// Gets or sets the variables collected or modified during step execution.
  /// </summary>
  public IReadOnlyDictionary<string, object> Vars { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
