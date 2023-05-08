namespace ConfirmSteps.Net.Tests;

using System.Diagnostics;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;

public abstract class HttpStepTestBase
{
    protected WireMockServer? Server { get; private set; }

    [SetUp]
    public void SetUp()
    {
        WireMockServerSettings settings = new()
        {
            Port = 54321,
            Logger = new WireMockConsoleLogger(),
        };

        Server = WireMockServer.Start(settings);
    }

    [TearDown]
    public void TearDown()
    {
        Server?.Dispose();
    }
}