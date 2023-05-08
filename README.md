<!-- markdownlint-disable MD033 MD041 -->
<div align="center">

<img src="motivation.png" alt="ConfirmSteps.Net" width="150px"/>

# ConfirmSteps.Net

[![Nuget](https://img.shields.io/nuget/v/ConfirmSteps.Net)](https://www.nuget.org/packages/ConfirmSteps.Net/)
[![Coverage Status](https://coveralls.io/repos/github/gcelet/ConfirmSteps.Net/badge.svg?branch=main&kill_cache=1)](https://coveralls.io/github/gcelet/ConfirmSteps.Net?branch=main&kill_cache=1)

A simple testing suite to confirm the sequence of steps
</div>

## What is ConfirmSteps.Net

ConfirmSteps.Net is a simple library to verify the correct sequence of steps with a focus on Rest APIs.


## How do I get started?

Write a sequence of steps to confirm inside a unit test with your favorite testing framework:

```csharp
    [Test]
    public async Task GettingStartedWithConfirmStepsNet()
    {
        // Arrange
        const string baseUrl = "{{url}}";
        Scenario<HttpScenarioData> scenario = Scenario.New<HttpScenarioData>("Getting started with ConfirmSteps.Net")
                .WithGlobals(e => e
                    .UseConst("url", "https://jsonplaceholder.typicode.com")
                    .UseObject("userId", d => d.UserId)
                )
                .WithSteps(s => s
                    .HttpStep("[Step-001-User-Read]", // GET https://jsonplaceholder.typicode.com/users/1
                        () => RequestBuilder.Get(baseUrl).AppendPathSegments("users", "{{userId}}"),
                        step => step
                            .VerifyJson((response, _) =>
                            {
                                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                            })
                            .Extract(e => e
                                .ToVars("website", FromJsonBodyToString("$.website"))
                            )
                    )
                    .WaitStep(100, 200)
                    .HttpStep("[Step-002-Todo-Add]", // POST https://jsonplaceholder.typicode.com/todos
                        () => RequestBuilder.Post(baseUrl).AppendPathSegment("todos")
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
                                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
                                Assert.AreEqual("Update home page of hildegard.org", response.SelectString("$.title"));
                            })
                            .Extract(e => e
                                .ToData(d => d.ToDoId!, FromJsonBodyToNumber("$.id"))
                            )
                    )
                )
                .Build()
            ;
        HttpScenarioData data = new()
        {
            UserId = 1,
        };

        // Act
        ConfirmStepResult<HttpScenarioData> confirmResult = await scenario.ConfirmSteps(data, CancellationToken.None);

        // Assert
        Assert.AreEqual(ConfirmStatus.Success, confirmResult.Status);
        Assert.AreEqual(201, confirmResult.Data.ToDoId);
    }

```

### Where can I get it?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [ConfirmSteps.Net](https://www.nuget.org/packages/ConfirmSteps.Net/) from the package manager console:

```
PM> Install-Package ConfirmSteps.Net
```
Or from the .NET CLI as:
```
dotnet add package ConfirmSteps.Net
```

### Do you have an issue?

If you're still running into problems, file an issue above.

### License, etc.

ConfirmSteps.Net is Copyright &copy; 2023 Grégory Célet and other contributors under the [MIT license](LICENSE).

[Motivation icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/motivation "motivation icons")
