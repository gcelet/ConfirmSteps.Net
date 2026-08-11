namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Templating;

using static CancellationExtensions;

[TestFixture]
public class StrictTemplateRenderingTests : HttpStepTestBase
{
    /// <summary>
    /// A template can say what it expects without rendering anything, which is what makes the whole
    /// set of missing variables reportable at once — and what lets a caller answer "what does this
    /// step consume?" before a run.
    /// </summary>
    [Test]
    public void ATemplateShouldNameTheVariablesItExpects()
    {
        // Arrange
        TemplateString template = new("api/shops/{{SHOP_ID}}/users/{{USER_ID}}?tag={{ SHOP_ID }}");

        // Act
        IReadOnlyList<string> names = template.ParameterNames;

        // Assert
        names.Should().Equal("SHOP_ID", "USER_ID");
    }

    [Test]
    public void ATemplateWithoutPlaceholdersShouldExpectNothing()
    {
        // Arrange
        TemplateString template = new("api/configuration");

        // Act & Assert
        template.ParameterNames.Should().BeEmpty();
    }

    /// <summary>
    /// Rendering itself stays lenient, and that is deliberate: the same <see cref="TemplateString"/>
    /// renders the execution summary, where a placeholder without a value is worth printing as it
    /// stands rather than losing the whole line. Strictness belongs to building a request, where the
    /// placeholder would otherwise go out on the wire.
    /// </summary>
    [Test]
    public void RenderingShouldStayLenientSoReportTemplatesKeepWorking()
    {
        // Arrange
        TemplateString template = new("Scenario \"{{ScenarioTitle}}\" — {{NotProvided}}");
        Dictionary<string, object> vars = new(StringComparer.Ordinal)
        {
            ["ScenarioTitle"] = "checkout",
        };

        // Act
        string rendered = template.Render(vars);

        // Assert
        rendered.Should().Be("Scenario \"checkout\" — {{NotProvided}}");
    }

    /// <summary>
    /// The behaviour this change exists for. Before, the placeholder went out url-encoded and the
    /// server answered 400 or 404 — reported as the system under test misbehaving rather than as the
    /// broken correlation chain it was.
    /// </summary>
    [Test]
    public async Task AStepShouldFailWhenItsRequestExpectsAVariableThatHasNoValue()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<StrictData> scenario = Scenario.New<StrictData>("[Scenario-Strict-0001]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users/{{USER_ID}}",
                    () => RequestBuilder.Get().AppendPathSegments("users", "{{USER_ID}}"),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<StrictData> result = await scenario.ConfirmSteps(new StrictData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);
        result.StepResults[0].Exception.Should().BeOfType<UnresolvedTemplateVariableException>();

        UnresolvedTemplateVariableException exception =
            (UnresolvedTemplateVariableException)result.StepResults[0].Exception!;

        exception.Unresolved.Should().HaveCount(1);
        exception.Unresolved[0].Name.Should().Be("USER_ID");
        exception.Unresolved[0].Location.Should().Be("path segment 2");
        exception.Message.Should().Contain("USER_ID").And.Contain("path segment 2");

        // The request never went out: refusing early is the point.
        Server.LogEntries.Should().BeEmpty();
    }

    /// <summary>
    /// All of them at once. A descriptor missing three variables should say three, not send its
    /// author round the loop three times.
    /// </summary>
    [Test]
    public async Task EveryMissingVariableShouldBeReportedTogetherWithWhereItWasExpected()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<StrictData> scenario = Scenario.New<StrictData>("[Scenario-Strict-0002]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-POST-/search",
                    () => RequestBuilder.Post()
                        .AppendPathSegments("shops", "{{SHOP_ID}}", "search")
                        .WithQueryString(q => q.Append("page", "{{PAGE}}"))
                        .WithHeaders(h => h.Header("Accept-language", "{{CULTURE}}"))
                        .WithBody("""{"text":"{{SEARCH_TEXT}}"}"""),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<StrictData> result = await scenario.ConfirmSteps(new StrictData(), cts.Token);

        // Assert
        UnresolvedTemplateVariableException exception =
            (UnresolvedTemplateVariableException)result.StepResults[0].Exception!;

        exception.Unresolved.Select(u => u.Name).Should()
            .BeEquivalentTo("SHOP_ID", "PAGE", "CULTURE", "SEARCH_TEXT");
        exception.Unresolved.Select(u => u.Location).Should()
            .BeEquivalentTo("path segment 2", "query 'page'", "header 'Accept-language'", "body");
    }

    /// <summary>
    /// Rendering already treats a null value as no value, so refusing to build must agree with it.
    /// </summary>
    [Test]
    public async Task AVariablePresentButNullShouldCountAsMissing()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<StrictData> scenario = Scenario.New<StrictData>("[Scenario-Strict-0003]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("USER_ID", _ => null!))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users/{{USER_ID}}",
                    () => RequestBuilder.Get().AppendPathSegments("users", "{{USER_ID}}"),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<StrictData> result = await scenario.ConfirmSteps(new StrictData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);
        result.StepResults[0].Exception.Should().BeOfType<UnresolvedTemplateVariableException>();
    }

    /// <summary>
    /// An empty value is a value. Only a missing variable and a null one are refused: a query
    /// parameter legitimately carries an empty string, and blocking the request over it would refuse
    /// a request the server accepts.
    /// </summary>
    [Test]
    public async Task AnEmptyValueShouldNotBlockTheRequest()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<StrictData> scenario = Scenario.New<StrictData>("[Scenario-Strict-0005]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("SEARCH", _ => string.Empty))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users",
                    () => RequestBuilder.Get()
                        .AppendPathSegment("users")
                        .WithQueryString(q => q.Append("search", "{{SEARCH}}")),
                    step => step.Verify((r, _) => r.IsSuccessStatusCode.Should().BeTrue())))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<StrictData> result = await scenario.ConfirmSteps(new StrictData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Success);
        Server.LogEntries.Should().HaveCount(1);
        Server.LogEntries.Single().RequestMessage.RawQuery.Should().Be("?search=");
    }

    /// <summary>
    /// A request whose variables all have values behaves exactly as before.
    /// </summary>
    [Test]
    public async Task AFullyResolvedRequestShouldBeUnaffected()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<StrictData> scenario = Scenario.New<StrictData>("[Scenario-Strict-0004]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("RESOURCE", _ => "users"))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users",
                    () => RequestBuilder.Get().AppendPathSegment("{{RESOURCE}}"),
                    step => step.Verify((r, _) => r.IsSuccessStatusCode.Should().BeTrue())))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<StrictData> result = await scenario.ConfirmSteps(new StrictData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Success);
        Server.LogEntries.Should().HaveCount(1);
    }

    public class StrictData
    {
    }
}
