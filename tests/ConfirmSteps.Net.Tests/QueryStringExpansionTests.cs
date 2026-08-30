namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;

using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Templating;

using static CancellationExtensions;

[TestFixture]
public class QueryStringExpansionTests : HttpStepTestBase
{
    /// <summary>
    /// The point of the whole thing: the number of values is a property of the data, so a run can
    /// vary it — to measure what asking for thirty identifiers costs against five — without the step
    /// being described any differently.
    /// </summary>
    [TestCase(1)]
    [TestCase(5)]
    [TestCase(30)]
    public async Task AParameterShouldRepeatOncePerValueTheVariableCarries(int count)
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        string[] modelIds = [.. Enumerable.Range(1, count).Select(i => i.ToString())];

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<ExpansionData> scenario = Scenario.New<ExpansionData>($"[Scenario-Expand-{count}]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("MODEL_IDS", _ => modelIds))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users",
                    () => RequestBuilder.Get()
                        .AppendPathSegment("users")
                        .WithQueryString(q => q.Append("modelIds", "{{MODEL_IDS}}")),
                    step => step.Verify((r, _) => r.IsSuccessStatusCode.Should().BeTrue())))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<ExpansionData> result = await scenario.ConfirmSteps(new ExpansionData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Success);

        string expected = "?" + string.Join("&", modelIds.Select(id => $"modelIds={id}"));
        Server.ShouldHaveSingleRequest().RawQuery.Should().Be(expected);
    }

    /// <summary>
    /// The repeated parameter keeps its place among the others, and a single-valued parameter beside
    /// it is untouched.
    /// </summary>
    [Test]
    public async Task ARepeatedParameterShouldSitAmongTheOthersInOrder()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<ExpansionData> scenario = Scenario.New<ExpansionData>("[Scenario-Expand-Order]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("MODEL_IDS", _ => new List<string> { "9587", "4841" }))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users",
                    () => RequestBuilder.Get()
                        .AppendPathSegment("users")
                        .WithQueryString(q => q
                            .Append("page", "1")
                            .Append("modelIds", "{{MODEL_IDS}}")
                            .Append("range", "1-20")),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new ExpansionData(), cts.Token);

        // Assert
        Server.ShouldHaveSingleRequest().RawQuery.Should()
            .Be("?page=1&modelIds=9587&modelIds=4841&range=1-20");
    }

    /// <summary>
    /// The only case where a declared parameter disappears. A single value, empty string included,
    /// always produces one.
    /// </summary>
    [Test]
    public async Task AnEmptyCollectionShouldProduceNoParameterAtAll()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<ExpansionData> scenario = Scenario.New<ExpansionData>("[Scenario-Expand-Empty]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("MODEL_IDS", _ => Array.Empty<string>()))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users",
                    () => RequestBuilder.Get()
                        .AppendPathSegment("users")
                        .WithQueryString(q => q
                            .Append("modelIds", "{{MODEL_IDS}}")
                            .Append("page", "1")),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new ExpansionData(), cts.Token);

        // Assert
        Server.ShouldHaveSingleRequest().RawQuery.Should().Be("?page=1");
    }

    /// <summary>
    /// Numbers, not just strings — and each element rendered on its own, so that varying the count
    /// cannot also change the format.
    /// </summary>
    [Test]
    public async Task ElementsShouldBeRenderedIndividuallyWhateverTheirType()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<ExpansionData> scenario = Scenario.New<ExpansionData>("[Scenario-Expand-Typed]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("MODEL_IDS", _ => new[] { 9587, 4841 }))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users",
                    () => RequestBuilder.Get()
                        .AppendPathSegment("users")
                        .WithQueryString(q => q.Append("modelIds", "{{MODEL_IDS}}")),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new ExpansionData(), cts.Token);

        // Assert
        Server.ShouldHaveSingleRequest().RawQuery.Should().Be("?modelIds=9587&modelIds=4841");
    }

    /// <summary>
    /// A string is a value, never a collection of characters.
    /// </summary>
    [Test]
    public async Task AStringShouldNotBeTreatedAsACollection()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<ExpansionData> scenario = Scenario.New<ExpansionData>("[Scenario-Expand-String]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("SEARCH", _ => "frein"))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users",
                    () => RequestBuilder.Get()
                        .AppendPathSegment("users")
                        .WithQueryString(q => q.Append("search", "{{SEARCH}}")),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new ExpansionData(), cts.Token);

        // Assert
        Server.ShouldHaveSingleRequest().RawQuery.Should().Be("?search=frein");
    }

    /// <summary>
    /// Text built around a placeholder has no reading that makes sense for a list. It used to send
    /// the type name — <c>ids-System.String[]</c> — and be rejected for reasons pointing nowhere near
    /// the mistake.
    /// </summary>
    [Test]
    public async Task ACollectionInsideSurroundingTextShouldBeRefused()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<ExpansionData> scenario = Scenario.New<ExpansionData>("[Scenario-Expand-Refused]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g.UseObject("MODEL_IDS", _ => new[] { "9587", "4841" }))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-GET-/users",
                    () => RequestBuilder.Get()
                        .AppendPathSegment("users")
                        .WithQueryString(q => q.Append("modelIds", "ids-{{MODEL_IDS}}")),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<ExpansionData> result =
            await scenario.ConfirmSteps(new ExpansionData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);
        result.StepResults[0].Exception.Should().BeOfType<MultiValuedTemplateVariableException>()
            .Which.VariableName.Should().Be("MODEL_IDS");
        Server.LogEntries.Should().BeEmpty();
    }

    /// <summary>
    /// A template that is one placeholder and nothing else stands for a value; anything else can only
    /// produce text.
    /// </summary>
    [TestCase("{{MODEL_IDS}}", true)]
    [TestCase("{{ MODEL_IDS }}", true)]
    [TestCase("ids-{{MODEL_IDS}}", false)]
    [TestCase("{{A}}{{B}}", false)]
    [TestCase("plain", false)]
    public void ASinglePlaceholderShouldBeRecognisedForWhatItIs(string template, bool expected)
    {
        // Arrange
        TemplateString templateString = new(template);

        // Act & Assert
        templateString.IsSinglePlaceholder.Should().Be(expected);
    }

    public class ExpansionData
    {
    }
}
