namespace ConfirmSteps.Steps;

using System.Diagnostics;

/// <summary>
/// Provides profiling capabilities for steps, measuring the time spent in different sections.
/// </summary>
public sealed class StepProfiler
{
    private readonly List<StepSectionStat> stats = new();

    /// <summary>
    /// Gets the collected profiling statistics.
    /// </summary>
    public IReadOnlyList<StepSectionStat> Stats => stats.AsReadOnly();

    /// <summary>
    /// Starts profiling a section with the specified name.
    /// </summary>
    /// <param name="sectionName">The name of the section to profile.</param>
    /// <returns>An <see cref="IDisposable"/> that stops profiling when disposed.</returns>
    public IDisposable Profile(string sectionName)
    {
        return new StepSectionProfiler(sectionName, this);
    }

    private void Return(StepSectionProfiler stepSectionProfiler)
    {
        stats.Add(new StepSectionStat
        {
            SectionName = stepSectionProfiler.SectionName,
            Elapsed = stepSectionProfiler.Stopwatch.Elapsed
        });
    }

    /// <summary>
    /// Represents the profiling statistics for a single section.
    /// </summary>
    public sealed class StepSectionStat
    {
        /// <summary>
        /// Gets the time elapsed during the section's execution.
        /// </summary>
        public TimeSpan Elapsed { get; init; }

        /// <summary>
        /// Gets the name of the profiled section.
        /// </summary>
        public string SectionName { get; init; } = string.Empty;
    }

    private class StepSectionProfiler : IDisposable
    {
        public StepSectionProfiler(string sectionName, StepProfiler stepProfiler)
        {
            SectionName = sectionName;
            StepProfiler = stepProfiler;
            Stopwatch = new Stopwatch();

            Stopwatch.Start();
        }

        public string SectionName { get; }

        public StepProfiler StepProfiler { get; }

        public Stopwatch Stopwatch { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Stopwatch.Stop();
            StepProfiler.Return(this);
        }
    }
}
