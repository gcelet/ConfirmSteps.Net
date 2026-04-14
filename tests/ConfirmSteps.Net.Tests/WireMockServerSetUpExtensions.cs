namespace ConfirmSteps.Net.Tests;

using System.Net;
using System.Net.Mime;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

public static class WireMockServerSetUpExtensions
{
  public static void SetUpGetMonitoringHeartbeat(this WireMockServer? server,
    HttpStatusCode statusCode = HttpStatusCode.OK,
    string status = "OK")
  {
    string jsonBody = $$"""
                            {"status": "{{status}}"}
                        """;

    server?.Given(
      Request.Create()
        .WithPath("/monitoring/heartbeat")
        .UsingGet()
    ).RespondWith(
      Response.Create()
        .WithStatusCode(statusCode)
        .WithBody(jsonBody)
        .WithHeader(Microsoft.Net.Http.Headers.HeaderNames.ContentType, MediaTypeNames.Application.Json)
    );
  }

  public static void SetUpGetUser(this WireMockServer? server,
    HttpStatusCode statusCode = HttpStatusCode.OK,
    int userId = 1)
  {
    string jsonBody = $$"""
                            {"id": "{{userId}}"}
                        """;

    server?.Given(
      Request.Create()
        .WithPath($"/users/{userId}")
        .UsingGet()
    ).RespondWith(
      Response.Create()
        .WithStatusCode(statusCode)
        .WithBody(jsonBody)
        .WithHeader(Microsoft.Net.Http.Headers.HeaderNames.ContentType, MediaTypeNames.Application.Json)
    );
  }

  public static void SetUpGetUsers(this WireMockServer? server,
    HttpStatusCode statusCode = HttpStatusCode.PartialContent)
  {
    string jsonBody = $$"""
                            [{"id": 1}, {"id": 2}, {"id": 3}]
                        """;

    server?.Given(
      Request.Create()
        .WithPath($"/users")
        .UsingGet()
    ).RespondWith(
      Response.Create()
        .WithStatusCode(statusCode)
        .WithBody(jsonBody)
        .WithHeader(Microsoft.Net.Http.Headers.HeaderNames.ContentType, MediaTypeNames.Application.Json)
        .WithHeader(Microsoft.Net.Http.Headers.HeaderNames.AcceptRanges, "items")
        .WithHeader(Microsoft.Net.Http.Headers.HeaderNames.ContentRange, "items 1-3/25")
    );
  }

  public static void SetUpGetNotFoundProblem(this WireMockServer? server,
    string path, bool returnProblemContentType = true)
  {
    string jsonBody = GetProblemJson(HttpStatusCode.NotFound, title: "Not found", details: "Not found");

    server?.Given(
      Request.Create()
        .WithPath(path)
        .UsingGet()
    ).RespondWith(
      Response.Create()
        .WithStatusCode(HttpStatusCode.NotFound)
        .WithBody(jsonBody)
        .WithHeader(Microsoft.Net.Http.Headers.HeaderNames.ContentType,
          returnProblemContentType ? "application/problem+json" : MediaTypeNames.Application.Json)
    );
  }


  private static string GetProblemJson(HttpStatusCode status, string type = "https://confirmsteps.net",
    string title = "A problem occured", string details = "Some problem occured", string instance = "/")
  {
    string jsonBody = $$"""
                            {"status": {{(int)status}}, "type": "{{type}}", "title": "{{title}}", "details": "{{details}}", "instance": "{{instance}}"}
                        """;

    return jsonBody;
  }
}
