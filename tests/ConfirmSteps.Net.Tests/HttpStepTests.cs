namespace ConfirmSteps.Net.Tests;

using System.Net;
using System.Text.Json.Serialization;
using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.Problems;
using ConfirmSteps.Steps.Http.RequestBuilding;
using FluentAssertions;
using static CancellationExtensions;

[TestFixture]
public class HttpStepTests : HttpStepTestBase
{
    private static IEnumerable<TestCaseData> VerifyTestData()
    {
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = false,
            Verify = step => step.Verify((response, _) => { response.Should().HaveStatusCode(HttpStatusCode.OK); })
        }).SetName("Verify_Sync_NoProblem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = false,
            Verify = step => step.Verify(async (response, _, ct) =>
            {
                await Task.Delay(10, ct);
                response.Should().HaveStatusCode(HttpStatusCode.OK);
            })
        }).SetName("Verify_Async_NoProblem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = true,
            Verify = step => step.Verify((response, _) => { response.Should().HaveStatusCode(HttpStatusCode.NotFound); })
        }).SetName("Verify_Sync_Problem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = true,
            Verify = step => step.Verify(async (response, _, ct) =>
            {
                await Task.Delay(10, ct);
                response.Should().HaveStatusCode(HttpStatusCode.NotFound);
            })
        }).SetName("Verify_Async_Problem");

        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = false,
            Verify = step => step.VerifyJson((response, _) =>
            {
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.OK
                    });
            })
        }).SetName("VerifyJson_Sync_NoProblem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = false,
            Verify = step => step.VerifyJson(async (response, _, ct) =>
            {
                await Task.Delay(10, ct);
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.OK
                    });
            })
        }).SetName("VerifyJson_Async_NoProblem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = true,
            Verify = step => step.VerifyJson((response, _) =>
            {
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.NotFound
                    });
            })
        }).SetName("VerifyJson_Sync_Problem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = true,
            Verify = step => step.VerifyJson(async (response, _, ct) =>
            {
                await Task.Delay(10, ct);
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.NotFound
                    });
            })
        }).SetName("VerifyJson_Async_Problem");

        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = false,
            Verify = step => step.VerifyRestApiResult((response, _) =>
            {
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.OK,
                        HaveResult = true,
                        HaveProblem = false,
                    });
            })
        }).SetName("VerifyApiResult_Sync_NoProblem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = false,
            Verify = step => step.VerifyRestApiResult(async (response, _, ct) =>
            {
                await Task.Delay(10, ct);
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.OK,
                        HaveResult = true,
                        HaveProblem = false,
                    });
            })
        }).SetName("VerifyApiResult_Async_NoProblem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = true,
            Verify = step => step.VerifyRestApiResult((response, _) =>
            {
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        HaveResult = false,
                        HaveProblem = true,
                    });
            })
        }).SetName("VerifyApiResult_Sync_Problem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = true,
            Verify = step => step.VerifyRestApiResult(async (response, _, ct) =>
            {
                await Task.Delay(10, ct);
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        HaveResult = false,
                        HaveProblem = true,
                    });
            })
        }).SetName("VerifyApiResult_Async_Problem");

        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = false,
            Verify = step => step.VerifyRestApiResult<MonitoringHeartbeatResult>((response, _) =>
            {
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.OK,
                        HaveResult = true,
                        HaveProblem = false,
                        Result = new
                        {
                            Status = "OK"
                        },
                        Problem = (ProblemDetails)null!,
                    });
            })
        }).SetName("VerifyApiResult<T>_Sync_NoProblem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = false,
            Verify = step => step.VerifyRestApiResult<MonitoringHeartbeatResult>(async (response, _, ct) =>
            {
                await Task.Delay(10, ct);
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.OK,
                        HaveResult = true,
                        HaveProblem = false,
                        Result = new
                        {
                            Status = "OK"
                        },
                        Problem = (ProblemDetails)null!,
                    });
            })
        }).SetName("VerifyApiResult<T>_Async_NoProblem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = true,
            Verify = step => step.VerifyRestApiResult<MonitoringHeartbeatResult>((response, _) =>
            {
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        HaveResult = false,
                        HaveProblem = true,
                        Result = (MonitoringHeartbeatResult)null!,
                        Problem = new
                        {
                            Status = (int)HttpStatusCode.NotFound,
                        },
                    });
            })
        }).SetName("VerifyApiResult<T>_Sync_Problem");
        yield return new TestCaseData(new VerifyTestCaseData
        {
            ServerShouldReturnProblem = true,
            Verify = step => step.VerifyRestApiResult<MonitoringHeartbeatResult>(async (response, _, ct) =>
            {
                await Task.Delay(10, ct);
                response.Should().NotBeNull()
                    .And.BeEquivalentTo(new
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        HaveResult = false,
                        HaveProblem = true,
                        Result = (MonitoringHeartbeatResult)null!,
                        Problem = new
                        {
                            Status = (int)HttpStatusCode.NotFound,
                        },
                    });
            })
        }).SetName("VerifyApiResult<T>_Async_Problem");
    }

    [TestCaseSource(nameof(VerifyTestData))]
    public async Task SingleGetStepScenario_Should_Failed_WhenVerifyFailed(VerifyTestCaseData testCaseData)
    {
        if (Server == null)
        {
            Assert.Fail("WireMockServer is null");
            return;
        }

        // Arrange
        if (testCaseData.ServerShouldReturnProblem)
        {
            Server.SetUpGetMonitoringHeartbeat();
        }
        else
        {
            Server.SetUpGetNotFoundProblem("/monitoring/heartbeat");
        }

        HttpClient httpClient = Server.CreateClient();
        Scenario<MonitoringHeartbeatScenarioData> scenario = Scenario
                .New<MonitoringHeartbeatScenarioData>("[SingleGetStepScenario]-")
                .WithServices(s => s.AddExternalHttpClient(httpClient))
                .WithSteps(steps => steps
                    .HttpStep("[Step-01]-GET-/monitoring/heartbeat",
                        () => RequestBuilder.Get()
                            .AppendPathSegments("monitoring", "heartbeat"),
                        testCaseData.Verify
                    )
                )
                .Build()
            ;
        MonitoringHeartbeatScenarioData data = new();
        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<MonitoringHeartbeatScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Should().BeEquivalentTo(new
        {
            Status = ConfirmStatus.Failure,
        });
        confirmResult.Exception.Should().NotBeNull();
    }

    [TestCaseSource(nameof(VerifyTestData))]
    public async Task SingleGetStepScenario_Should_Succeed_WhenVerifySucceed(VerifyTestCaseData testCaseData)
    {
        if (Server == null)
        {
            Assert.Fail("WireMockServer is null");
            return;
        }

        // Arrange
        if (!testCaseData.ServerShouldReturnProblem)
        {
            Server.SetUpGetMonitoringHeartbeat();
        }
        else
        {
            Server.SetUpGetNotFoundProblem("/monitoring/heartbeat");
        }

        HttpClient httpClient = Server.CreateClient();
        Scenario<MonitoringHeartbeatScenarioData> scenario = Scenario
                .New<MonitoringHeartbeatScenarioData>("[SingleGetStepScenario]-")
                .WithServices(s => s.AddExternalHttpClient(httpClient))
                .WithSteps(steps => steps
                    .HttpStep("[Step-01]-GET-/monitoring/heartbeat",
                        () => RequestBuilder.Get()
                            .AppendPathSegments("monitoring", "heartbeat"),
                        testCaseData.Verify
                    )
                )
                .Build()
            ;
        MonitoringHeartbeatScenarioData data = new();
        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<MonitoringHeartbeatScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Should().BeEquivalentTo(new
        {
            Status = ConfirmStatus.Success,
            Exception = (Exception)null!,
        });
    }

    public class MonitoringHeartbeatResult
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class MonitoringHeartbeatScenarioData
    {
        public string? HeartbeatStatus { get; set; }
    }

    public class VerifyTestCaseData
    {
        public bool ServerShouldReturnProblem { get; set; }

        public Action<HttpStepBuilder<MonitoringHeartbeatScenarioData>> Verify { get; init; } = null!;
    }
}