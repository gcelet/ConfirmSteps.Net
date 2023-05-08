namespace ConfirmSteps.Steps;

using System.Diagnostics;

public sealed class StepProfiler
{
    private readonly List<StepSectionStat> _stats = new();

    public IReadOnlyList<StepSectionStat> Stats => _stats.AsReadOnly();

    public IDisposable Profile(string sectionName)
    {
        return new StepSectionProfiler(sectionName, this);
    }

    private void Return(StepSectionProfiler stepSectionProfiler)
    {
        _stats.Add(new StepSectionStat
        {
            SectionName = stepSectionProfiler.SectionName,
            Elapsed = stepSectionProfiler.Stopwatch.Elapsed
        });
    }

    public sealed class StepSectionStat
    {
        public TimeSpan Elapsed { get; init; }

        public string SectionName { get; init; } = String.Empty;
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