namespace ConfirmSteps.Net.Tests;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Steps.Http.Rest;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using static CancellationExtensions;
using static ConfirmSteps.Steps.Http.ResponseParsing.HttpResponseExtractors;

public class BasicSyntaxTests
{
    [Test]
    public async Task BasicCodeScenarioShouldConfirmSteps()
    {
        // Arrange
        Scenario<CodeScenarioData> scenario = Scenario.New<CodeScenarioData>("[Scenario-Code-0001]-Basic-Syntax")
                .WithSteps(s => s
                    .CodeStep("Add-2",
                        step => step.Execute(c =>
                        {
                            CodeScenarioData d = c.ScenarioContext.Data;

                            d.Value += 2;
                            d.Counter++;

                            return ConfirmStatus.Success;
                        })
                    )
                    .WaitStep(TimeSpan.FromMilliseconds(100))
                    .CodeStep("Multiply-By-4",
                        step => step.Execute(c =>
                        {
                            CodeScenarioData d = c.ScenarioContext.Data;

                            d.Value *= 4;
                            d.Counter++;

                            return ConfirmStatus.Success;
                        })
                    )
                    .WaitStep(TimeSpan.FromMilliseconds(100))
                    .CodeStep("Substract-5",
                        step => step.Execute(c =>
                        {
                            CodeScenarioData d = c.ScenarioContext.Data;

                            d.Value -= 5;
                            d.Counter++;

                            return ConfirmStatus.Success;
                        })
                    )
                )
                .Build()
            ;
        CodeScenarioData data = new()
        {
            Value = 10,
            Counter = 0
        };

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<CodeScenarioData> result = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        CodeScenarioData expectedData = new()
        {
            Value = 43,
            Counter = 3
        };
        data.Should().BeEquivalentTo(expectedData);
    }

    [Test]
    public async Task BasicHttpScenarioShouldConfirmSteps()
    {
        // Arrange
        const string baseUrl = "{{url}}";
        Scenario<HttpScenarioData> scenario = Scenario.New<HttpScenarioData>("[Scenario-Http-0001]-Basic-Syntax")
                .WithGlobals(e => e
                    .UseConst("url", "https://jsonplaceholder.typicode.com")
                    .UseConst("correlationId", "confirm-steps-dotnet-correlation-id")
                    .UseObject("userId", d => d.UserId)
                )
                .WithSteps(s => s
                    .HttpStep("[Step-001-User-Read]", // GET https://jsonplaceholder.typicode.com/users/1
                        () => RequestBuilder.Get(baseUrl).AppendPathSegments("users", "{{userId}}")
                            .WithQueryString(q => q.Append("correlationId", "{{correlationId}}"))
                            .WithHeaders(h => h.Header("X-CorrelationId", "{{correlationId}}")),
                        step => step
                            .VerifyJson((response, _) =>
                            {
                                response.StatusCode.Should().Be(HttpStatusCode.OK);

                                var data = new
                                {
                                    Id = response.SelectNumber("$.id"),
                                    Website = response.SelectString("$.website")
                                };

                                data.Should().BeEquivalentTo(new
                                {
                                    Id = 1L,
                                    Website = "hildegard.org"
                                });
                            })
                            .Extract(e => e
                                .ToVars("userId", FromJsonBodyToNumber("$.id"))
                                .ToVars("website", FromJsonBodyToString("$.website"))
                                .ToVars("userETag", FromHeaderToString("etag"))
                                .ToVars("rateLimitLimit", FromHeaderToNumber("x-ratelimit-limit"))
                            )
                    )
                    .WaitStep(100, 200)
                    .HttpStep("[Step-002-Todo-Add]", // POST https://jsonplaceholder.typicode.com/todos
                        () => RequestBuilder.Post(baseUrl).AppendPathSegment("todos")
                            .WithQueryString(q => q.Append("correlationId", "{{correlationId}}"))
                            .WithHeaders(h => h.Header("X-CorrelationId", "{{correlationId}}"))
                            .WithHeaders(h => h.Header(HeaderNames.ContentType, MediaTypeNames.Application.Json))
                            .WithBody(
                                """
                                {
                                    "userId": {{userId}},
                                    "title": "Update home page of {{website}}",
                                    "completed": false
                                }
                                """
                            ),
                        step => step
                            .VerifyJson((response, _) =>
                            {
                                response.StatusCode.Should().Be(HttpStatusCode.Created);
                                Assert.AreEqual("Update home page of hildegard.org", response.SelectString("$.title"));
                            })
                            .Extract(e => e
                                .ToVars("todoId", FromJsonBodyToNumber("$.id"))
                                .ToData(d => d.ToDoId!, FromJsonBodyToNumber("$.id"))
                            )
                    )
                )
                .Build()
            ;
        HttpScenarioData data = new()
        {
            UserId = 1L,
        };

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<HttpScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Should().BeEquivalentTo(new
        {
            Status = ConfirmStatus.Success,
            Data = new
            {
                UserId = 1L,
                ToDoId = 201L,
            },
            Vars = new Dictionary<string, object>
            {
                { "url", "https://jsonplaceholder.typicode.com"},
                { "correlationId", "confirm-steps-dotnet-correlation-id"},
                { "userId", 1m},
                { "website", "hildegard.org"},
                { "userETag", "W/\"1fd-+2Y3G3w049iSZtw5t1mzSnunngE\""},
                { "rateLimitLimit", 1000m},
                { "todoId", 201m},
            },
            Exception = (Exception)null!,
        });
    }

    public class CodeScenarioData
    {
        public long Counter { get; set; }

        public long Value { get; set; }
    }

    public class HttpScenarioData
    {
        public long? ToDoId { get; set; }

        public long UserId { get; set; }
    }
}