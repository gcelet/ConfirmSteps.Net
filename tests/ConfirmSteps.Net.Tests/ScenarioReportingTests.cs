namespace ConfirmSteps.Net.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mime;

using AwesomeAssertions;

using ConfirmSteps.Reporting;
using ConfirmSteps.Steps.Http.RequestBuilding;

using Microsoft.Net.Http.Headers;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.ResponseProviders;
using WireMock.Server;

using static ConfirmSteps.Steps.Http.ResponseParsing.HttpResponseExtractors;

[Explicit("These tests are meant to be run manually to verify the scenario execution summary reporting. They are not intended for automated test runs.")]
[TestFixtureSource(typeof(ScenarioExecutionSummaryReporterProviders), nameof(ScenarioExecutionSummaryReporterProviders.EnumerateTestFixtures))]
public class ScenarioReportingTests
{
  private ScenarioExecutionSummaryReporterProvider ScenarioExecutionSummaryReporterProvider { get; }

  public enum StepReturnType
  {
    ExpectToPass,

    NotFoundProblem,

    ValidationProblem,

    InternalServerErrorProblem,

    NotFoundEmptyResponse,

    BadRequestEmptyResponse,

    InternalServerErrorEmptyResponse,
  }

  public ScenarioReportingTests(ScenarioExecutionSummaryReporterProvider scenarioExecutionSummaryReporterProvider)
  {
    ScenarioExecutionSummaryReporterProvider = scenarioExecutionSummaryReporterProvider;
  }

  [TestCaseSource(typeof(TestCases), nameof(TestCases.EnumerateTestCases))]
  [CancelAfter(2_000)]
  public async Task Scenario_Should_Display_Summary_When_Failure(ScenarioReportingTestCaseData testCaseData,
    CancellationToken cancellationToken)
  {
    // Arrange
    using WireMockServer server = WireMockServer.Start();
    BlogPostScenarioData scenarioData = new()
    {
      UserId = testCaseData.UserId
    };

    ConfigureMockServer(server, testCaseData);

    // Act
    await RunScenario(testCaseData.TestName, scenarioData, server, CancellationToken.None);
  }

  private void ConfigureMockServer(WireMockServer server, ScenarioReportingTestCaseData testCaseData)
  {
    // Mock GET /users/1
    server.Given(Request.Create().WithPath($"/users/{testCaseData.UserId}").UsingGet())
      .RespondWith(ResponseProviderFactory.BuildResponseProvider(testCaseData.StepUserById, () =>
          Response.Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithBody("""{"id": 1, "name": "Leanne Graham"}""")
        )
      );

    // Mock POST /posts
    server.Given(Request.Create().WithPath("/posts").UsingPost())
      .RespondWith(ResponseProviderFactory.BuildResponseProvider(testCaseData.StepPostCreate, () =>
          Response.Create()
            .WithStatusCode(HttpStatusCode.Created)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithBody("""{"id": 101}""")
        )
      );

    // Mock GET /posts/101
    server.Given(Request.Create().WithPath("/posts/101").UsingGet())
      .RespondWith(ResponseProviderFactory.BuildResponseProvider(testCaseData.StepPostGetById, () =>
          Response.Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithBody(
              """{"id": 101, "userId": 1, "title": "Learning ConfirmSteps.Net", "body": "This is a great library for integration testing."}""")
        )
      );

    // Mock POST /comments
    server.Given(Request.Create().WithPath("/comments").UsingPost())
      .RespondWith(ResponseProviderFactory.BuildResponseProvider(testCaseData.StepCommentCreate, () =>
          Response.Create()
            .WithStatusCode(HttpStatusCode.Created)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithBody("""{"id": 501}""")
        )
      );
  }


  private async Task RunScenario(string scenarioTitle,
    BlogPostScenarioData scenarioData, WireMockServer server,
    CancellationToken cancellationToken)
  {
    // Arrange
    string baseUrl = server.Urls[0];
    Scenario<BlogPostScenarioData> scenario = Scenario.New<BlogPostScenarioData>(scenarioTitle)
      .WithGlobals(g => g
        .UseConst("baseUrl", baseUrl)
        .UseObject("userId", d => d.UserId)
        .UseObject("postTitle", d => d.PostTitle)
        .UseObject("postBody", d => d.PostBody)
      )
      .WithSteps(s => s
        // Step 1: Read User Info (HTTP Read)
        .HttpStep("[Step-001-Get-User]",
          () => RequestBuilder.Get("{{baseUrl}}").AppendPathSegments("users", "{{userId}}"),
          step => step
            .Verify((res, _) => res.StatusCode.Should().Be(HttpStatusCode.OK))
            .Extract(e => e
              .ToData(d => d.UserName, FromJsonBodyToString("$.name"))
            )
        )
        // Step 2: Prepare Post Content (Code Step)
        .CodeStep("[Step-002-Prepare-Content]", step => step.Execute(context =>
        {
          BlogPostScenarioData data = context.ScenarioContext.Data;

          data.PostTitle = $"{data.UserName}'s First Post: {data.PostTitle}";

          return ConfirmStatus.Success;
        }))
        // Step 3: Create Post (HTTP Write)
        .HttpStep("[Step-003-Create-Post]",
          () => RequestBuilder.Post("{{baseUrl}}").AppendPathSegment("posts")
            .WithBody(
              """
              {
                  "userId": {{userId}},
                  "title": "{{postTitle}}",
                  "body": "{{postBody}}"
              }
              """
            ),
          step => step
            .VerifyJson((res, _) =>
            {
              res.StatusCode.Should().Be(HttpStatusCode.Created);
            })
            .Extract(e => e
              .ToData(d => d.CreatedPostId, FromJsonBodyToNumber("$.id"))
              .ToVars("postId", FromJsonBodyToNumber("$.id"))
            )
        )
        // Step 4: Wait Step (Does not count in the 6 steps)
        .WaitStep("[Step-004-Wait-Created-Post]", TimeSpan.FromMilliseconds(100))
        // Step 5: Verify Post Exists (HTTP Read)
        .HttpStep("[Step-005-Fetch-Created-Post]",
          () => RequestBuilder.Get("{{baseUrl}}").AppendPathSegments("posts", "{{postId}}"),
          step => step.VerifyRestApiResult((res, _) =>
          {
            res.StatusCode.Should().Be(HttpStatusCode.OK);
          })
        )
        // Step 6: Validate Data (Code Step)
        .CodeStep("[Step-006-Validate-Post-State]", step => step.Execute(context =>
        {
          BlogPostScenarioData data = context.ScenarioContext.Data;

          if (data.CreatedPostId == 101 && !string.IsNullOrEmpty(data.UserName))
          {
            data.PostVerified = true;

            return ConfirmStatus.Success;
          }

          return ConfirmStatus.Failure;
        }))
        // Step 7: Add Comment to the post (HTTP Write)
        .HttpStep("[Step-007-Add-Comment]",
          () => RequestBuilder.Post("{{baseUrl}}").AppendPathSegment("comments")
            .WithBody(
              """
              {
                  "postId": {{postId}},
                  "name": "Great post!",
                  "email": "test@example.com",
                  "body": "Thanks for sharing this information."
              }
              """
            ),
          step => step.VerifyRestApiResult((res, _) =>
          {
            res.StatusCode.Should().Be(HttpStatusCode.Created);
          })
        )
      )
      .Build();

    // Act
    ConfirmStepResult<BlogPostScenarioData> result = await scenario.ConfirmSteps(scenarioData, cancellationToken);
    // Assert
    string executionSummary = result.GetExecutionSummary(ScenarioExecutionSummaryReporterProvider.Configure);
    Assert.That(result.Status, Is.EqualTo(ConfirmStatus.Success), executionSummary);
    await TestContext.Out.WriteLineAsync(executionSummary);
  }

  public class ScenarioReportingTestCaseData
  {
    public required StepReturnType StepCommentCreate { get; init; }

    public required StepReturnType StepPostCreate { get; init; }

    public required StepReturnType StepPostGetById { get; init; }

    public required StepReturnType StepUserById { get; init; }

    public required string TestName { get; init; }

    public required long UserId { get; init; }
  }

  private class BlogPostScenarioData
  {
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public long CreatedPostId { get; set; }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public string PostBody { get; set; } = "This is a great library for integration testing.";

    public string PostTitle { get; set; } = "Learning ConfirmSteps.Net";

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public bool PostVerified { get; set; }

    public long UserId { get; init; }

    public string UserName { get; set; } = string.Empty;
  }

  private static class ResponseProviderFactory
  {
    public static IResponseProvider BuildResponseProvider(StepReturnType stepReturnType, Func<IResponseProvider> validResponse)
    {
      if (stepReturnType == StepReturnType.ExpectToPass)
      {
        return validResponse();
      }

      if (stepReturnType == StepReturnType.NotFoundProblem)
      {
        return BuildJsonProblemResponseProvider(HttpStatusCode.NotFound);
      }

      if (stepReturnType == StepReturnType.ValidationProblem)
      {
        // TODO: Build a more realistic validation problem response
        return Response.Create();
      }

      if (stepReturnType == StepReturnType.InternalServerErrorProblem)
      {
        return BuildJsonProblemResponseProvider(HttpStatusCode.InternalServerError);
      }

      if (stepReturnType == StepReturnType.NotFoundEmptyResponse)
      {
        return Response.Create()
            .WithStatusCode(HttpStatusCode.NotFound)
            .WithBody(string.Empty)
          ;
      }

      if (stepReturnType == StepReturnType.BadRequestEmptyResponse)
      {
        return Response.Create()
            .WithStatusCode(HttpStatusCode.BadRequest)
            .WithBody(string.Empty)
          ;
      }

      if (stepReturnType == StepReturnType.InternalServerErrorEmptyResponse)
      {
        return Response.Create()
            .WithStatusCode(HttpStatusCode.InternalServerError)
            .WithBody(string.Empty)
          ;
      }

      return Response.Create();
    }

    private static IResponseProvider BuildJsonProblemResponseProvider(HttpStatusCode statusCode,
      string type = "https://confirmsteps.net", string title = "A problem occured",
      string details = "Some problem occured", string instance = "/")
    {
      string jsonBody = $$"""
                              {"status": {{(int)statusCode}}, "type": "{{type}}", "title": "{{title}}", "details": "{{details}}", "instance": "{{instance}}"}
                          """;

      return Response.Create()
          .WithStatusCode(statusCode)
#if NET6_0
          .WithHeader(HeaderNames.ContentType, "application/problem+json")
#else
          .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.ProblemJson)
#endif
          .WithBody(jsonBody)
        ;
    }
  }

  private static class TestCases
  {
    public static IEnumerable<TestCaseData> EnumerateTestCases()
    {
      yield return BuildTestCaseData("000-All-Steps-Pass");
      // UserById step fails in different ways
      yield return BuildTestCaseData("001-HttpResponseMessage-Step-001-UserById-Failed-NotFoundProblem",
        stepUserById: StepReturnType.NotFoundProblem
      );

      yield return BuildTestCaseData("001-HttpResponseMessage-Step-001-UserById-Failed-ValidationProblem",
        stepUserById: StepReturnType.ValidationProblem
      );

      yield return BuildTestCaseData("001-HttpResponseMessage-Step-001-UserById-Failed-InternalServerErrorProblem",
        stepUserById: StepReturnType.InternalServerErrorProblem
      );

      yield return BuildTestCaseData("001-HttpResponseMessage-Step-001-UserById-Failed-NotFoundEmptyResponse",
        stepUserById: StepReturnType.NotFoundEmptyResponse
      );

      yield return BuildTestCaseData("001-HttpResponseMessage-Step-001-UserById-Failed-BadRequestEmptyResponse",
        stepUserById: StepReturnType.BadRequestEmptyResponse
      );

      yield return BuildTestCaseData("001-HttpResponseMessage-Step-001-UserById-Failed-InternalServerErrorEmptyResponse",
        stepUserById: StepReturnType.InternalServerErrorEmptyResponse
      );

      // PostCreate step fails in different ways
      yield return BuildTestCaseData("002-HttpResponseJson-Step-003-PostCreate-Failed-NotFoundProblem",
        stepPostCreate: StepReturnType.NotFoundProblem
      );

      yield return BuildTestCaseData("002-HttpResponseJson-Step-003-PostCreate-Failed-ValidationProblem",
        stepPostCreate: StepReturnType.ValidationProblem
      );

      yield return BuildTestCaseData("002-HttpResponseJson-Step-003-PostCreate-Failed-InternalServerErrorProblem",
        stepPostCreate: StepReturnType.InternalServerErrorProblem
      );

      yield return BuildTestCaseData("002-HttpResponseJson-Step-003-PostCreate-Failed-NotFoundEmptyResponse",
        stepPostCreate: StepReturnType.NotFoundEmptyResponse
      );

      yield return BuildTestCaseData("002-HttpResponseJson-Step-003-PostCreate-Failed-BadRequestEmptyResponse",
        stepPostCreate: StepReturnType.BadRequestEmptyResponse
      );

      yield return BuildTestCaseData("002-HttpResponseJson-Step-003-PostCreate-Failed-InternalServerErrorEmptyResponse",
        stepPostCreate: StepReturnType.InternalServerErrorEmptyResponse
      );

      // PostGetById step fails in different ways
      yield return BuildTestCaseData("003-RestApiResult-Step-005-PostGetById-Failed-NotFoundProblem",
        stepPostGetById: StepReturnType.NotFoundProblem
      );

      yield return BuildTestCaseData("003-RestApiResult-Step-005-PostGetById-Failed-ValidationProblem",
        stepPostGetById: StepReturnType.ValidationProblem
      );

      yield return BuildTestCaseData("003-RestApiResult-Step-005-PostGetById-Failed-InternalServerErrorProblem",
        stepPostGetById: StepReturnType.InternalServerErrorProblem
      );

      yield return BuildTestCaseData("003-RestApiResult-Step-005-PostGetById-Failed-NotFoundEmptyResponse",
        stepPostGetById: StepReturnType.NotFoundEmptyResponse
      );

      yield return BuildTestCaseData("003-RestApiResult-Step-005-PostGetById-Failed-BadRequestEmptyResponse",
        stepPostGetById: StepReturnType.BadRequestEmptyResponse
      );

      yield return BuildTestCaseData("003-RestApiResult-Step-005-PostGetById-Failed-InternalServerErrorEmptyResponse",
        stepPostGetById: StepReturnType.InternalServerErrorEmptyResponse
      );

      // CommentCreate step fails in different ways
      yield return BuildTestCaseData("004-RestApiResultTyped-Step-007-CommentCreate-Failed-NotFoundProblem",
        stepCommentCreate: StepReturnType.NotFoundProblem
      );

      yield return BuildTestCaseData("004-RestApiResultTyped-Step-007-CommentCreate-Failed-ValidationProblem",
        stepCommentCreate: StepReturnType.ValidationProblem
      );

      yield return BuildTestCaseData("004-RestApiResultTyped-Step-007-CommentCreate-Failed-InternalServerErrorProblem",
        stepCommentCreate: StepReturnType.InternalServerErrorProblem
      );

      yield return BuildTestCaseData("004-RestApiResultTyped-Step-007-CommentCreate-Failed-NotFoundEmptyResponse",
        stepCommentCreate: StepReturnType.NotFoundEmptyResponse
      );

      yield return BuildTestCaseData("004-RestApiResultTyped-Step-007-CommentCreate-Failed-BadRequestEmptyResponse",
        stepCommentCreate: StepReturnType.BadRequestEmptyResponse
      );

      yield return BuildTestCaseData("004-RestApiResultTyped-Step-007-CommentCreate-Failed-InternalServerErrorEmptyResponse",
        stepCommentCreate: StepReturnType.InternalServerErrorEmptyResponse
      );
    }

    private static TestCaseData BuildTestCaseData(string testName,
      StepReturnType stepUserById = StepReturnType.ExpectToPass,
      StepReturnType stepPostCreate = StepReturnType.ExpectToPass,
      StepReturnType stepPostGetById = StepReturnType.ExpectToPass,
      StepReturnType stepCommentCreate = StepReturnType.ExpectToPass,
      long userId = 1L)
    {
      ScenarioReportingTestCaseData testCaseData = new()
      {
        TestName = testName,
        StepUserById = stepUserById,
        StepPostCreate = stepPostCreate,
        StepPostGetById = stepPostGetById,
        StepCommentCreate = stepCommentCreate,
        UserId = userId,
      };

      return new TestCaseData(testCaseData)
          .SetName(testName)
        ;
    }
  }
}
