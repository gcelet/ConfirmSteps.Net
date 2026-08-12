namespace ConfirmSteps.Net.Tests;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

using AwesomeAssertions;

using ConfirmSteps.Data;
using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.RequestBuilding;
using ConfirmSteps.Templating;

using static CancellationExtensions;

[TestFixture]
public class JsonBodyTemplateTests : HttpStepTestBase
{
    /// <summary>
    /// The reason to describe a body as a structure rather than as text: escaping is the
    /// serialiser's job. A value carrying quotes, a backslash and a newline cannot break the document
    /// — nor inject one — which a hand-written text template cannot promise.
    /// </summary>
    [Test]
    public async Task AHostileValueShouldNotBeAbleToBreakTheDocument()
    {
        // Arrange
        const string hostile = """say "hi" \ then {"injected": true} """;

        JsonObject body = await SendBody(
            new JsonObject { ["searchText"] = "{{SEARCH}}" },
            g => g.UseObject("SEARCH", _ => hostile));

        // Assert
        body["searchText"]!.GetValue<string>().Should().Be(hostile);
        body.Count.Should().Be(1);
    }

    /// <summary>
    /// What a text template cannot express: the value keeps the type of its variable. The quotes
    /// around a placeholder are how one is written inside JSON, not a claim about the result.
    /// </summary>
    [Test]
    public async Task AValueShouldTakeTheTypeOfItsVariable()
    {
        // Arrange
        JsonObject body = await SendBody(
            new JsonObject
            {
                ["shopId"] = "{{SHOP_ID}}",
                ["active"] = "{{ACTIVE}}",
                ["ratio"] = "{{RATIO}}",
                ["label"] = "{{LABEL}}",
            },
            g => g
                .UseObject("SHOP_ID", _ => 1149)
                .UseObject("ACTIVE", _ => true)
                .UseObject("RATIO", _ => 1.5m)
                .UseObject("LABEL", _ => "frein"));

        // Assert
        body["shopId"]!.GetValueKind().Should().Be(JsonValueKind.Number);
        body["shopId"]!.GetValue<int>().Should().Be(1149);
        body["active"]!.GetValueKind().Should().Be(JsonValueKind.True);
        body["ratio"]!.GetValue<decimal>().Should().Be(1.5m);
        body["label"]!.GetValueKind().Should().Be(JsonValueKind.String);
    }

    /// <summary>
    /// A number written the same way whatever the machine's culture. String interpolation would have
    /// produced 1,5 under fr-FR and sent a body the server cannot read.
    /// </summary>
    [Test]
    public async Task ANumberShouldBeWrittenInvariantlyWhateverTheCulture()
    {
        // Arrange
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        try
        {
            JsonObject body = await SendBody(
                new JsonObject { ["ratio"] = "{{RATIO}}" },
                g => g.UseObject("RATIO", _ => 1.5d));

            // Assert
            body.ToJsonString().Should().Contain("1.5").And.NotContain("1,5");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// Three different requests, and an endpoint that reads them as three: a property left out, a
    /// property sent as null, and a property with a value. A template could express the last two;
    /// leaving one out is what this adds, and it is what drives the behaviour of a search endpoint.
    /// </summary>
    [Test]
    public async Task AnOptionalPropertyShouldBeLeftOutWhenItsVariableHasNoValue()
    {
        // Arrange
        JsonObject body = await SendBody(
            new JsonObject
            {
                ["searchText?"] = "{{SEARCH}}",
                ["vehicleId?"] = "{{VEHICLE_ID}}",
                ["sortType"] = "BY_STOCK",
                ["explicitlyNull"] = null,
            },
            g => g.UseObject("SEARCH", _ => "frein"));

        // Assert
        body["searchText"]!.GetValue<string>().Should().Be("frein");
        body.Should().NotContainKey("vehicleId");
        body.Should().NotContainKey("vehicleId?");
        body["sortType"]!.GetValue<string>().Should().Be("BY_STOCK");
        body.Should().ContainKey("explicitlyNull");
        body["explicitlyNull"].Should().BeNull();
    }

    /// <summary>
    /// An empty value is still a value, here as everywhere: it is sent, because the endpoint reads an
    /// empty search text and no search text differently.
    /// </summary>
    [Test]
    public async Task AnOptionalPropertyWithAnEmptyValueShouldStillBeSent()
    {
        // Arrange
        JsonObject body = await SendBody(
            new JsonObject { ["searchText?"] = "{{SEARCH}}" },
            g => g.UseObject("SEARCH", _ => string.Empty));

        // Assert
        body.Should().ContainKey("searchText");
        body["searchText"]!.GetValue<string>().Should().BeEmpty();
    }

    /// <summary>
    /// A missing variable in an optional property is not a failure: not having a value is what the
    /// marker is for. A required one still refuses to build the request.
    /// </summary>
    [Test]
    public async Task AnOptionalPropertyShouldNotFailTheRequest()
    {
        // Arrange
        JsonObject body = await SendBody(
            new JsonObject { ["searchText?"] = "{{NEVER_SET}}", ["page"] = 1 },
            _ => { });

        // Assert
        body.Should().NotContainKey("searchText");
        body["page"]!.GetValue<int>().Should().Be(1);
    }

    [Test]
    public async Task ACollectionShouldBecomeAnArray()
    {
        // Arrange
        JsonObject body = await SendBody(
            new JsonObject { ["modelIds"] = "{{MODEL_IDS}}" },
            g => g.UseObject("MODEL_IDS", _ => new[] { 9587, 4841, 4771 }));

        // Assert
        body["modelIds"]!.GetValueKind().Should().Be(JsonValueKind.Array);
        body["modelIds"]!.AsArray().Select(n => n!.GetValue<int>()).Should().Equal(9587, 4841, 4771);
    }

    [Test]
    public async Task NestedObjectsAndArraysShouldBeWalked()
    {
        // Arrange
        JsonObject body = await SendBody(
            new JsonObject
            {
                ["customer"] = new JsonObject
                {
                    ["shopId"] = "{{SHOP_ID}}",
                    ["tags"] = new JsonArray("fixed", "{{LABEL}}"),
                },
                ["page"] = 1,
            },
            g => g.UseObject("SHOP_ID", _ => 1149).UseObject("LABEL", _ => "vip"));

        // Assert
        body["customer"]!["shopId"]!.GetValue<int>().Should().Be(1149);
        body["customer"]!["tags"]!.AsArray().Select(n => n!.GetValue<string>())
            .Should().Equal("fixed", "vip");
        body["page"]!.GetValue<int>().Should().Be(1);
    }

    /// <summary>
    /// A placeholder inside surrounding text is text, as everywhere else.
    /// </summary>
    [Test]
    public async Task APlaceholderInsideTextShouldRenderAsText()
    {
        // Arrange
        JsonObject body = await SendBody(
            new JsonObject { ["authorization"] = "Bearer {{TOKEN}}" },
            g => g.UseObject("TOKEN", _ => "abc"));

        // Assert
        body["authorization"]!.GetValue<string>().Should().Be("Bearer abc");
    }

    /// <summary>
    /// Reported with the place in the document that wanted it, which is what says which line of a
    /// descriptor to look at.
    /// </summary>
    [Test]
    public async Task AMissingVariableShouldBeReportedWithItsPathInTheDocument()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        JsonObject template = new()
        {
            ["customer"] = new JsonObject { ["shopId"] = "{{SHOP_ID}}" },
            ["items"] = new JsonArray(new JsonObject { ["id"] = "{{ITEM_ID}}" }),
        };

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<BodyData> scenario = Scenario.New<BodyData>("[Scenario-Body-Missing]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-POST-/users",
                    () => RequestBuilder.Post().AppendPathSegment("users").WithJsonBody(template),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<BodyData> result = await scenario.ConfirmSteps(new BodyData(), cts.Token);

        // Assert
        UnresolvedTemplateVariableException exception =
            (UnresolvedTemplateVariableException)result.StepResults[0].Exception!;

        exception.Unresolved.Select(u => u.Location).Should()
            .BeEquivalentTo("body $.customer.shopId", "body $.items[0].id");
        Server.LogEntries.Should().BeEmpty();
    }

    [Test]
    public async Task AJsonBodyShouldDefaultToTheJsonContentType()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<BodyData> scenario = Scenario.New<BodyData>("[Scenario-Body-ContentType]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-POST-/users",
                    () => RequestBuilder.Post()
                        .AppendPathSegment("users")
                        .WithJsonBody(new JsonObject { ["page"] = 1 }),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new BodyData(), cts.Token);

        // Assert
        Server.LogEntries.Single().RequestMessage.Headers!["Content-Type"].Single()
            .Should().StartWith("application/json");
    }

    /// <summary>
    /// A request has one body. Declaring both ways is an authoring mistake, and it is refused where
    /// it is made rather than by whichever one happened to win.
    /// </summary>
    [Test]
    public void DeclaringBothKindsOfBodyShouldBeRefused()
    {
        // Arrange
        RequestBuilder builder = RequestBuilder.Post().WithJsonBody(new JsonObject());

        // Act
        Action act = () => builder.WithBody("{}");

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*one body*");
    }

    private async Task<JsonObject> SendBody(JsonNode template,
        Action<VarBuilder<BodyData>> globals)
    {
        if (Server == null)
        {
            throw new InvalidOperationException("The stub server did not start.");
        }

        Server.SetUpGetUsers();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<BodyData> scenario = Scenario.New<BodyData>("[Scenario-Body]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(globals)
            .WithSteps(steps => steps
                .HttpStep("[Step-01]-POST-/users",
                    () => RequestBuilder.Post().AppendPathSegment("users").WithJsonBody(template),
                    step => step.Verify((_, _) => { })))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        ConfirmStepResult<BodyData> result = await scenario.ConfirmSteps(new BodyData(), cts.Token);

        result.Status.Should().Be(ConfirmStatus.Success);

        string sent = Server.LogEntries.Single().RequestMessage.Body!;

        return JsonNode.Parse(sent)!.AsObject();
    }

    public class BodyData
    {
    }
}
