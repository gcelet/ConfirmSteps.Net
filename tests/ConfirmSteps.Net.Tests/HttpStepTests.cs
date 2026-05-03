namespace ConfirmSteps.Net.Tests;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json.Serialization;

using AwesomeAssertions;
using AwesomeAssertions.Execution;

using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.Problems;
using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Steps.Http.ResponseVerification;

using static CancellationExtensions;

[TestFixture]
public class HttpStepTests : HttpStepTestBase
{
  private static IEnumerable<TestCaseData> VerifyTestData()
  {
    // The test cases cover various combinations of server responses (problem vs no problem)
    // and verification methods (synchronous vs asynchronous, and different verification approaches).
    // Each test case is designed to validate that the verification logic correctly identifies the expected outcomes
    // based on the server's response, ensuring that both success and failure scenarios are thoroughly tested.
    // The test cases include:
    // 1. Synchronous verification with no problem (expecting HTTP 200 OK).
    // 2. Asynchronous verification with no problem (expecting HTTP 200 OK).
    // 3. Synchronous verification with a problem (expecting HTTP 404 Not Found).
    // 4. Asynchronous verification with a problem (expecting HTTP 404 Not Found).
    // 5. Synchronous JSON verification with no problem (expecting a JSON response with HTTP 200 OK).
    // 6. Asynchronous JSON verification with no problem (expecting a JSON response with HTTP 200 OK).
    // 7. Synchronous JSON verification with a problem (expecting a JSON response with HTTP 404 Not Found).
    // 8. Asynchronous JSON verification with a problem (expecting a JSON response with HTTP 404 Not Found).
    // 9. Synchronous REST API result verification with no problem (expecting a REST API result indicating success).
    // 10. Asynchronous REST API result verification with no problem (expecting a REST API result indicating success).
    // 11. Synchronous REST API result verification with a problem (expecting a REST API result indicating failure).
    // 12. Asynchronous REST API result verification with a problem (expecting a REST API result indicating failure).

    yield return new TestCaseData(new VerifyTestCaseData
      {
        ServerShouldReturnProblem = false,
        Verify = step => step.Verify((response, _) => { response.StatusCode.Should().Be(HttpStatusCode.OK); })
      })
      .SetName("Verify_Sync_NoProblem");

    yield return new TestCaseData(new VerifyTestCaseData
      {
        ServerShouldReturnProblem = false,
        Verify = step => step.Verify(async (response, _, ct) =>
        {
          await Task.Delay(10, ct);
          response.StatusCode.Should().Be(HttpStatusCode.OK);
        })
      })
      .SetName("Verify_Async_NoProblem");

    yield return new TestCaseData(new VerifyTestCaseData
      {
        ServerShouldReturnProblem = true,
        Verify = step => step.Verify((response, _) => { response.StatusCode.Should().Be(HttpStatusCode.NotFound); })
      })
      .SetName("Verify_Sync_Problem");

    yield return new TestCaseData(new VerifyTestCaseData
      {
        ServerShouldReturnProblem = true,
        Verify = step => step.Verify(async (response, _, ct) =>
        {
          await Task.Delay(10, ct);
          response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        })
      })
      .SetName("Verify_Async_Problem");

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
      })
      .SetName("VerifyJson_Sync_NoProblem");

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
      })
      .SetName("VerifyJson_Async_NoProblem");

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
      })
      .SetName("VerifyJson_Sync_Problem");

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
      })
      .SetName("VerifyJson_Async_Problem");

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
      })
      .SetName("VerifyApiResult_Sync_NoProblem");

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
      })
      .SetName("VerifyApiResult_Async_NoProblem");

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
      })
      .SetName("VerifyApiResult_Sync_Problem");

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
      })
      .SetName("VerifyApiResult_Async_Problem");

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
      })
      .SetName("VerifyApiResult<T>_Sync_NoProblem");

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
      })
      .SetName("VerifyApiResult<T>_Async_NoProblem");

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
      })
      .SetName("VerifyApiResult<T>_Sync_Problem");

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
      })
      .SetName("VerifyApiResult<T>_Async_Problem");
  }

  [Test]
  public async Task SingleGetStepScenario_Should_AllowToVerifyAllResponseHeaders()
  {
    if (Server == null)
    {
      Assert.Fail("WireMockServer is null");
      return;
    }

    // Arrange
    const string acceptRangesValue = "items";
    const string contentRangeValue = "items 1-3/25";
    Server.SetUpGetUsers();

    HttpClient httpClient = Server.CreateClient();
    Scenario<MonitoringHeartbeatScenarioData> scenario = Scenario
        .New<MonitoringHeartbeatScenarioData>("[HttpStepAllowToVerifyAllResponseHeaders]")
        .WithServices(s => s.AddExternalHttpClient(httpClient))
        .WithSteps(steps => steps
          .HttpStep("[Step-01]-GET-/users",
            () => RequestBuilder.Get().AppendPathSegment("users"),
            step => step
              .Verify((r, _) =>
              {
                r.Content.Should().NotBeNull();
                r.Content.Headers.Should().NotBeNull();
                r.Content.Headers.ContentType.Should().NotBeNull().And.Subject.ToString().Should()
                  .Be(MediaTypeNames.Application.Json);

                r.Headers.AcceptRanges.Should().Contain(acceptRangesValue);
                r.Content.Headers.ContentRange.Should().NotBeNull().And.Subject.ToString().Should().Be(contentRangeValue);
              })
              .VerifyJson((r, _) =>
              {
                r.Headers.Should().NotBeNull();
                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.ContentType).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.ContentType).Should()
                  .Contain(MediaTypeNames.Application.Json);

                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges).Should().Contain(acceptRangesValue);
                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.ContentRange).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.ContentRange).Should().Contain(contentRangeValue);
              })
              .VerifyRestApiResult((r, _) =>
              {
                r.Headers.Should().NotBeNull();
                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.ContentType).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.ContentType)
                  .Should().Contain(MediaTypeNames.Application.Json);

                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges).Should().Contain(acceptRangesValue);
                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.ContentRange).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.ContentRange).Should().Contain(contentRangeValue);
              })
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

  [Test]
  public async Task SingleGetStepScenario_Should_RunAllVerifiersWhenUsingDefaultModeEvenIfAllVerifiersSucceed()
  {
    if (Server == null)
    {
      Assert.Fail("WireMockServer is null");
      return;
    }

    // Arrange
    Server.SetUpGetUser();

    HttpClient httpClient = Server.CreateClient();
    Scenario<MonitoringHeartbeatScenarioData> scenario = Scenario
        .New<MonitoringHeartbeatScenarioData>("[HttpStepRunAllVerifiersWhenUsingDefaultModeEvenIfAllVerifiersSucceed]")
        .WithServices(s => s.AddExternalHttpClient(httpClient))
        .WithSteps(steps => steps
          .HttpStep("[Step-01]-GET-/users/1",
            () => RequestBuilder.Get().AppendPathSegments("users", "1"),
            step => step
              .Verify((_, s) =>
              {
                s.Vars[nameof(HttpStepBuilder<>.Verify)] = true;
              })
              .VerifyJson((_, s) =>
              {
                s.Vars[nameof(HttpStepBuilder<>.VerifyJson)] = true;
              })
              .VerifyRestApiResult((_, s) =>
              {
                s.Vars[nameof(HttpStepBuilder<>.VerifyRestApiResult)] = true;
              })
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
      Vars = new Dictionary<string, object>
      {
        { nameof(HttpStepBuilder<>.Verify), true },
        { nameof(HttpStepBuilder<>.VerifyJson), true },
        { nameof(HttpStepBuilder<>.VerifyRestApiResult), true },
      },
      Exception = (Exception)null!,
    });
  }

  [Test]
  public async Task SingleGetStepScenario_Should_RunAllVerifiersWhenUsingVerifyAllModeEvenIfSomeVerifiersFail()
  {
    if (Server == null)
    {
      Assert.Fail("WireMockServer is null");
      return;
    }

    // Arrange
    Server.SetUpGetUser();

    HttpClient httpClient = Server.CreateClient();
    Scenario<MonitoringHeartbeatScenarioData> scenario = Scenario
        .New<MonitoringHeartbeatScenarioData>("[HttpStepRunAllVerifiersWhenUsingVerifyAllModeEvenIfSomeVerifiersFail]")
        .WithServices(s => s.AddExternalHttpClient(httpClient))
        .WithSteps(steps => steps
          .HttpStep("[Step-01]-GET-/users/1",
            () => RequestBuilder.Get().AppendPathSegments("users", "1"),
            step => step
              .WithVerificationMode(HttpResponseVerificationMode.VerifyAll)
              .Verify((_, _) => throw new Exception("Fail from HttpResponseMessage verification"))
              .VerifyJson((_, _) => throw new Exception("Fail from HttpResponseJson verification"))
              .VerifyRestApiResult((_, _) => throw new Exception("Fail from RestApiResult verification"))
          )
        )
        .Build()
      ;

    MonitoringHeartbeatScenarioData data = new();
    // Act
    using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
    ConfirmStepResult<MonitoringHeartbeatScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

    // Assert
    using AssertionScope scope = new();
    confirmResult.Status.Should().Be(ConfirmStatus.Failure);
    confirmResult.Exception.Should().NotBeNull();
    confirmResult.Exception.Should().BeOfType<AggregateException>()
      .Subject.InnerExceptions.Should().HaveCount(3)
      .And.SatisfyRespectively(
        httpResponseMessageException =>
          httpResponseMessageException.Message.Should().Be("Fail from HttpResponseMessage verification"),
        httpResponseJsonException => httpResponseJsonException.Message.Should().Be("Fail from HttpResponseJson verification"),
        restApiResultException => restApiResultException.Message.Should().Be("Fail from RestApiResult verification")
      )
      ;
  }

  [Test]
  public async Task SingleStepScenario_Should_AllowNonRfcHeaders()
  {
    if (Server == null)
    {
      Assert.Fail("WireMockServer is null");
      return;
    }

    // Arrange
    const string acceptRangesValue = "500";
    const string contentRangeValue = "1-3/25";
    Server.SetUpGetUsers(acceptRanges: acceptRangesValue, contentRange: contentRangeValue);

    HttpClient httpClient = Server.CreateClient();
    Scenario<MonitoringHeartbeatScenarioData> scenario = Scenario
        .New<MonitoringHeartbeatScenarioData>("[HttpStepAllowNonRfcHeaders]")
        .WithServices(s => s.AddExternalHttpClient(httpClient))
        .WithSteps(steps => steps
          .HttpStep("[Step-01]-GET-/users",
            () => RequestBuilder.Get().AppendPathSegment("users"),
            step => step
              .Verify((r, _) =>
              {
                r.Content.Should().NotBeNull();
                r.Content.Headers.Should().NotBeNull();
                r.Content.Headers.ContentType.Should().NotBeNull().And.Subject.ToString().Should()
                  .Be(MediaTypeNames.Application.Json);

                bool containsAcceptRangeHeader = r.Headers.NonValidated.TryGetValues(
                  Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges, out HeaderStringValues acceptRangesValues);

                containsAcceptRangeHeader.Should().BeTrue();
                acceptRangesValues.Should().Contain(acceptRangesValue);

                bool containsContentRangeHeader = r.Content.Headers.NonValidated.TryGetValues(
                  Microsoft.Net.Http.Headers.HeaderNames.ContentRange, out HeaderStringValues contentRangeValues);

                containsContentRangeHeader.Should().BeTrue();
                contentRangeValues.Should().Contain(contentRangeValue);
              })
              .VerifyJson((r, _) =>
              {
                r.Headers.Should().NotBeNull();
                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.ContentType).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.ContentType).Should()
                  .Contain(MediaTypeNames.Application.Json);

                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges).Should().Contain(acceptRangesValue);
                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.ContentRange).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.ContentRange).Should().Contain(contentRangeValue);
              })
              .VerifyRestApiResult((r, _) =>
              {
                r.Headers.Should().NotBeNull();
                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.ContentType).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.ContentType)
                  .Should().Contain(MediaTypeNames.Application.Json);

                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges).Should().Contain(acceptRangesValue);
                r.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.ContentRange).Should().BeTrue();
                r.Headers.GetValues(Microsoft.Net.Http.Headers.HeaderNames.ContentRange).Should().Contain(contentRangeValue);
              })
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
        .New<MonitoringHeartbeatScenarioData>("[HttpStepScenarioFailedWhenVerifyFailed]")
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
        .New<MonitoringHeartbeatScenarioData>("[HttpStepSucceed_WhenVerifySucceed]")
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
