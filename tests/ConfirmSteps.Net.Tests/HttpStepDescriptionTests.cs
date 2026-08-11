namespace ConfirmSteps.Net.Tests;

using System.Net;
using System.Text.Json.Nodes;

using AwesomeAssertions;

using ConfirmSteps.Steps.Http;
using ConfirmSteps.Steps.Http.Json;

using static CancellationExtensions;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

[TestFixture]
public class HttpStepDescriptionTests : HttpStepTestBase
{
    /// <summary>
    /// The case the whole thing exists for, taken from a real application: one step lists courtesy
    /// vehicles and extracts their model identifiers, and a later step searches on them. Nothing in
    /// either description says how many there will be — the data decides, so a run can vary it.
    /// </summary>
    [TestCase(2)]
    [TestCase(7)]
    public async Task AValueExtractedByOneStepShouldFeedALaterOne(int modelCount)
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        int[] modelIds = [.. Enumerable.Range(1, modelCount).Select(i => 9000 + i)];

        Server
            .Given(Request.Create().WithPath("/api/courtesyvehicles").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(new JsonObject
                {
                    ["items"] = new JsonArray(
                        [.. modelIds.Select(id => (JsonNode)new JsonObject { ["modelId"] = id })]),
                }.ToJsonString()));

        Server
            .Given(Request.Create().WithPath("/api/vehicles/models/search").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"models":[]}"""));

        HttpStepDescription list = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "courtesyvehicles.list",
              "title": "Courtesy vehicles",
              "request": {
                "method": "GET",
                "path": "api/courtesyvehicles",
                "query": [ { "name": "pageRange", "value": "1-20" } ]
              },
              "extract": [
                {
                  "var": "MODEL_IDS",
                  "path": "$.items[*].modelId",
                  "as": "numberList",
                  "required": true
                }
              ],
              "verify": [ { "kind": "status", "expect": [200] } ]
            }
            """)!);

        HttpStepDescription search = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "vehicles.models.search",
              "request": {
                "method": "GET",
                "path": "api/vehicles/models/search",
                "query": [ { "name": "modelIds", "value": "{{MODEL_IDS}}" } ]
              },
              "verify": [ { "kind": "status", "expect": [200] } ]
            }
            """)!);

        HttpStepVerifierRegistry<CatalogData> registry = StatusRegistry();

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<CatalogData> scenario = Scenario.New<CatalogData>("[Scenario-Described]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps
                .HttpStep(list, registry)
                .HttpStep(search, registry))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<CatalogData> result = await scenario.ConfirmSteps(new CatalogData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Success);
        result.StepResults[0].Title.Should().Be("Courtesy vehicles");
        result.StepResults[1].Title.Should().Be("vehicles.models.search");

        string expected = string.Join("&", modelIds.Select(id => $"modelIds={id}"));
        Server.LogEntries.Last().RequestMessage.RawQuery.Should().Be("?" + expected);
    }

    /// <summary>
    /// The chain is said to be broken where it breaks, rather than several steps later as a server
    /// error nobody can trace back.
    /// </summary>
    [Test]
    public async Task ARequiredExtractionThatFindsNothingShouldFailTheStep()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server
            .Given(Request.Create().WithPath("/api/courtesyvehicles").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"items":[]}"""));

        HttpStepDescription list = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "courtesyvehicles.list",
              "request": { "method": "GET", "path": "api/courtesyvehicles" },
              "extract": [
                { "var": "MODEL_IDS", "path": "$.items[*].modelId", "as": "numberList", "required": true }
              ]
            }
            """)!);

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<CatalogData> scenario = Scenario.New<CatalogData>("[Scenario-Required]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps.HttpStep(list, StatusRegistry()))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<CatalogData> result = await scenario.ConfirmSteps(new CatalogData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);
        result.StepResults[0].Exception.Should().BeOfType<RequiredExtractionFailedException>();
        ((RequiredExtractionFailedException)result.StepResults[0].Exception!).VariableName
            .Should().Be("MODEL_IDS");
    }

    /// <summary>
    /// An extraction that is not required stays silent, which is what it was before and what a step
    /// with a fallback wants.
    /// </summary>
    [Test]
    public async Task AnOptionalExtractionThatFindsNothingShouldNotFailTheStep()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server
            .Given(Request.Create().WithPath("/api/courtesyvehicles").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"items":[]}"""));

        HttpStepDescription list = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "courtesyvehicles.list",
              "request": { "method": "GET", "path": "api/courtesyvehicles" },
              "extract": [ { "var": "MODEL_IDS", "path": "$.items[*].modelId", "as": "numberList" } ]
            }
            """)!);

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<CatalogData> scenario = Scenario.New<CatalogData>("[Scenario-Optional]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps.HttpStep(list, StatusRegistry()))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<CatalogData> result = await scenario.ConfirmSteps(new CatalogData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Success);
    }

    /// <summary>
    /// A step that expects a 500 is a legitimate step, and the library has no say in it: the host's
    /// verification decides, so 200 is the failure here.
    /// </summary>
    [Test]
    public async Task TheHostDecidesWhichStatusIsASuccess()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server
            .Given(Request.Create().WithPath("/api/configuration").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));

        HttpStepDescription expectsFailure = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "configuration.broken",
              "request": { "method": "GET", "path": "api/configuration" },
              "verify": [ { "kind": "status", "expect": [500] } ]
            }
            """)!);

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<CatalogData> scenario = Scenario.New<CatalogData>("[Scenario-Expect-500]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps.HttpStep(expectsFailure, StatusRegistry()))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<CatalogData> result = await scenario.ConfirmSteps(new CatalogData(), cts.Token);

        // Assert
        result.Status.Should().Be(ConfirmStatus.Failure);
        result.StepResults[0].Exception!.Message.Should().Contain("500").And.Contain("200");
    }

    /// <summary>
    /// Refused when the step is built, not discovered when it runs — and the message lists what is
    /// available, because a typo is the likeliest cause.
    /// </summary>
    [Test]
    public void AVerificationKindNothingRegisteredShouldBeRefusedAtBuildTime()
    {
        // Arrange
        HttpStepDescription description = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "some.step",
              "request": { "method": "GET", "path": "api/configuration" },
              "verify": [ { "kind": "statusCode" } ]
            }
            """)!);

        // Act
        Action act = () => Scenario.New<CatalogData>("[Scenario-Unknown-Kind]")
            .WithSteps(steps => steps.HttpStep(description, StatusRegistry()))
            .Build();

        // Assert
        act.Should().Throw<HttpStepDescriptionException>()
            .WithMessage("*statusCode*").WithMessage("*status*");
    }

    [Test]
    public void ADescriptionWithoutARequestShouldBeRefused()
    {
        // Arrange
        JsonNode document = JsonNode.Parse("""{ "id": "some.step" }""")!;

        // Act
        Action act = () => HttpStepDescription.FromJson(document);

        // Assert
        act.Should().Throw<HttpStepDescriptionException>().WithMessage("*$.request*");
    }

    /// <summary>
    /// A host reads its own properties off the document it supplied: the library keeps what it does
    /// not understand rather than rejecting it, which is what lets a catalogue carry its own concerns.
    /// </summary>
    [Test]
    public void ADescriptionShouldKeepThePropertiesTheLibraryDoesNotRead()
    {
        // Arrange
        HttpStepDescription description = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "catalog.search",
              "category": "Catalogue",
              "dataRoles": [ "catalogSearch" ],
              "request": { "method": "POST", "path": "api/catalog/search" }
            }
            """)!);

        // Act & Assert
        description.Id.Should().Be("catalog.search");
        description.Title.Should().Be("catalog.search");
        description.Document["category"]!.GetValue<string>().Should().Be("Catalogue");
        description.Document["dataRoles"]!.AsArray().Should().HaveCount(1);
    }

    /// <summary>
    /// A host that numbers its steps supplies the title, which is the metrics dimension of many a
    /// report.
    /// </summary>
    [Test]
    public async Task AHostShouldBeAbleToTitleADescribedStepItsOwnWay()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server
            .Given(Request.Create().WithPath("/api/configuration").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));

        HttpStepDescription description = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "configuration",
              "title": "Configuration",
              "request": { "method": "GET", "path": "api/configuration" }
            }
            """)!);

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<CatalogData> scenario = Scenario.New<CatalogData>("[Scenario-Titled]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithSteps(steps => steps.HttpStep(description, StatusRegistry(), "[Step-002]-configuration"))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        ConfirmStepResult<CatalogData> result = await scenario.ConfirmSteps(new CatalogData(), cts.Token);

        // Assert
        result.StepResults[0].Title.Should().Be("[Step-002]-configuration");
    }

    /// <summary>
    /// A body declared as a structure, templated and serialised — the escaping guarantee reaching a
    /// described step.
    /// </summary>
    [Test]
    public async Task ADescribedBodyShouldBeBuiltAndSerialised()
    {
        // Arrange
        if (Server == null)
        {
            Assert.Fail("The stub server did not start.");

            return;
        }

        Server
            .Given(Request.Create().WithPath("/api/catalog/search").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));

        HttpStepDescription description = HttpStepDescription.FromJson(JsonNode.Parse("""
            {
              "id": "catalog.search",
              "request": {
                "method": "POST",
                "path": "api/catalog/search",
                "headers": { "Accept-language": "{{CULTURE_CODE}}" },
                "body": { "searchText": "{{SEARCH_TEXT}}", "shopId": "{{SHOP_ID}}" }
              }
            }
            """)!);

        using HttpClient httpClient = Server.CreateClient();
        using Scenario<CatalogData> scenario = Scenario.New<CatalogData>("[Scenario-Described-Body]")
            .WithServices(s => s.AddExternalHttpClient(httpClient))
            .WithGlobals(g => g
                .UseObject("SEARCH_TEXT", _ => """say "hi" \ now""")
                .UseObject("SHOP_ID", _ => 1149)
                .UseObject("CULTURE_CODE", _ => "fr-FR"))
            .WithSteps(steps => steps.HttpStep(description, StatusRegistry()))
            .Build();

        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();

        // Act
        await scenario.ConfirmSteps(new CatalogData(), cts.Token);

        // Assert
        JsonObject sent = JsonNode.Parse(Server.LogEntries.Single().RequestMessage.Body!)!.AsObject();

        sent["searchText"]!.GetValue<string>().Should().Be("""say "hi" \ now""");
        sent["shopId"]!.GetValue<int>().Should().Be(1149);
        // Written "Accept-language" in the description and canonicalised to "Accept-Language" on the
        // wire by HttpRequestHeaders, which knows the standard spelling. Worth knowing before someone
        // asserts on the casing they typed.
        Server.LogEntries.Single().RequestMessage.Headers!
            .Should().ContainKey("Accept-Language")
            .WhoseValue.Single().Should().Be("fr-FR");
    }

    /// <summary>
    /// The status check a load harness would register: the library ships none, the host brings its
    /// own, written with whatever assertion library it already uses.
    /// </summary>
    private static HttpStepVerifierRegistry<CatalogData> StatusRegistry()
        => new HttpStepVerifierRegistry<CatalogData>().Register("status", entry =>
        {
            int[] accepted = [.. entry["expect"]?.AsArray().Select(n => n!.GetValue<int>()) ?? []];

            return (response, _, _) =>
            {
                int actual = (int)response.StatusCode;

                if (accepted.Length > 0 && !accepted.Contains(actual))
                {
                    throw new InvalidOperationException(
                        $"Expected one of {string.Join(", ", accepted)} but received {actual}.");
                }

                return Task.CompletedTask;
            };
        });

    public class CatalogData
    {
    }
}
