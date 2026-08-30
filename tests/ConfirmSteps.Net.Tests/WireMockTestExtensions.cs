namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using WireMock;
using WireMock.Logging;
using WireMock.Server;

/// <summary>
/// Extension methods for <see cref="WireMockServer"/> assertions in tests.
/// </summary>
public static class WireMockTestExtensions
{
    /// <summary>
    /// Asserts that the WireMock server received exactly one request and returns the validated log entry.
    /// </summary>
    /// <param name="server">The <see cref="WireMockServer"/> instance under test.</param>
    /// <param name="because">A formatted explanation of why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because"/>.</param>
    /// <returns>The single <see cref="ILogEntry"/> recorded by the server.</returns>
    public static ILogEntry ShouldHaveSingleLogEntry(
        this WireMockServer? server,
        string because = "",
        params object[] becauseArgs)
    {
        server.Should().NotBeNull("the WireMock server must be initialized and started");
        ArgumentNullException.ThrowIfNull(server);

        server.LogEntries.Should().ContainSingle(because, becauseArgs);

        ILogEntry entry = server.LogEntries.First();
        entry.RequestMessage.Should().NotBeNull("the recorded log entry must contain a non-null RequestMessage");

        return entry;
    }

    /// <summary>
    /// Asserts that the WireMock server received exactly one request and returns its non-null <see cref="IRequestMessage"/>.
    /// </summary>
    /// <param name="server">The <see cref="WireMockServer"/> instance under test.</param>
    /// <param name="because">A formatted explanation of why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because"/>.</param>
    /// <returns>The guaranteed non-null <see cref="IRequestMessage"/> of the single recorded request.</returns>
    public static IRequestMessage ShouldHaveSingleRequest(
        this WireMockServer? server,
        string because = "",
        params object[] becauseArgs)
    {
        ILogEntry entry = server.ShouldHaveSingleLogEntry(because, becauseArgs);
        if (entry.RequestMessage is null)
        {
            throw new InvalidOperationException("The recorded log entry does not contain a RequestMessage.");
        }

        return entry.RequestMessage;
    }

    /// <summary>
    /// Asserts that the WireMock server received at least one request and returns its last non-null <see cref="IRequestMessage"/>.
    /// </summary>
    /// <param name="server">The <see cref="WireMockServer"/> instance under test.</param>
    /// <param name="because">A formatted explanation of why the assertion is needed.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because"/>.</param>
    /// <returns>The guaranteed non-null <see cref="IRequestMessage"/> of the last recorded request.</returns>
    public static IRequestMessage ShouldHaveLastRequest(
        this WireMockServer? server,
        string because = "",
        params object[] becauseArgs)
    {
        server.Should().NotBeNull("the WireMock server must be initialized and started");
        ArgumentNullException.ThrowIfNull(server);

        server.LogEntries.Should().NotBeEmpty(because, becauseArgs);

        ILogEntry entry = server.LogEntries.Last();
        if (entry.RequestMessage is null)
        {
            throw new InvalidOperationException("The recorded log entry does not contain a RequestMessage.");
        }

        return entry.RequestMessage;
    }
}
