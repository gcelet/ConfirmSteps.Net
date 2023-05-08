namespace ConfirmSteps.Net.Tests;

using System.Diagnostics;

public static class CancellationExtensions
{
    public static CancellationTokenSource CreateDefaultScenarioCancellationTokenSource(TimeSpan? timeout = null)
    {
        TimeSpan useTimeout = timeout.GetValueOrDefault(TimeSpan.FromSeconds(30));

        return Debugger.IsAttached ? new CancellationTokenSource() : new CancellationTokenSource(useTimeout);
    }
}