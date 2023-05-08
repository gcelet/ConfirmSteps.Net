namespace ConfirmSteps.Net.Tests.Templating;

using ConfirmSteps.Templating;
using FluentAssertions;

[TestFixture]
public class TemplateStringTests
{
    private static IEnumerable<TestCaseData> RenderTestData()
    {
        yield return new TestCaseData(new TemplateStringTestCaseData("fixed-string", "fixed-string"));
        yield return new TestCaseData(new TemplateStringTestCaseData("{{param}}", "dynamic-string")
        {
            Vars = { { "param", "dynamic-string" } }
        });
        yield return new TestCaseData(new TemplateStringTestCaseData("{{param}}", "{{param}}"));
        yield return new TestCaseData(
            new TemplateStringTestCaseData("[before]{{param}}[after]", "[before]dynamic-string[after]")
            {
                Vars = { { "param", "dynamic-string" } }
            });
        yield return new TestCaseData(
            new TemplateStringTestCaseData("{{param1}}|{{param2}}", "dynamic-string-1|dynamic-string-2")
            {
                Vars =
                {
                    { "param1", "dynamic-string-1" },
                    { "param2", "dynamic-string-2" }
                }
            });
        yield return new TestCaseData(new TemplateStringTestCaseData("[before]{{param1}}|{{param2}}[after]",
            "[before]dynamic-string-1|dynamic-string-2[after]")
        {
            Vars =
            {
                { "param1", "dynamic-string-1" },
                { "param2", "dynamic-string-2" }
            }
        });
    }

    [TestCaseSource(nameof(RenderTestData))]
    public void Render_Should_Return(TemplateStringTestCaseData testCaseData)
    {
        // Arrange
        TemplateString templateString = new(testCaseData.Template);
        // Act
        string actualValue = templateString.Render(testCaseData.Vars);
        // Assert
        actualValue.Should().Be(testCaseData.ExpectedValue);
    }

    public class TemplateStringTestCaseData
    {
        public TemplateStringTestCaseData(string template, string expectedValue)
        {
            Template = template;
            ExpectedValue = expectedValue;
        }

        public string ExpectedValue { get; }

        public string Template { get; }

        public Dictionary<string, object> Vars { get; } = new();
    }
}