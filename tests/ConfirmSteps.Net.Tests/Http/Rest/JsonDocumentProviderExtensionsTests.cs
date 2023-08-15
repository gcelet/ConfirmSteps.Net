namespace ConfirmSteps.Net.Tests.Http.Rest;

using System.Text.Json.Nodes;
using ConfirmSteps.Steps.Http.Rest;
using FluentAssertions;

[TestFixture]
public class JsonDocumentProviderExtensionsTests
{
    [TestCaseSource(typeof(TestCases), nameof(TestCases.SelectBooleanTestCases))]
    public void SelectBoolean_Should_Return(JsonPathTestCaseData<bool?> testCaseData)
    {
        // Arrange
        HttpResponseJson httpResponseJson = SampleJsonDocuments.BuildSample1();
        // Act
        bool? selectedBoolean = testCaseData.Extractor(httpResponseJson);
        // Assert
        if (testCaseData.ExpectedNbItems == 1)
        {
            selectedBoolean.Should().Be(testCaseData.ExpectedResult);
        }
        else if (testCaseData.ExpectedNbItems == 0)
        {
            selectedBoolean.Should().BeNull();
        }
        else
        {
            Assert.Fail("SelectBoolean return at most one boolean!");
        }
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.SelectBooleanArrayTestCases))]
    public void SelectBooleanArray_Should_Return(JsonPathTestCaseData<bool?[]?> testCaseData)
    {
        // Arrange
        HttpResponseJson httpResponseJson = SampleJsonDocuments.BuildSample1();
        // Act
        bool?[]? selectedBooleans = testCaseData.Extractor(httpResponseJson);
        // Assert
        if (testCaseData.ExpectedNbItems > 0)
        {
            selectedBooleans.Should()
                .HaveCount(testCaseData.ExpectedNbItems)
                .And.ContainInOrder(testCaseData.ExpectedResult);
        }
        else
        {
            selectedBooleans.Should().BeNull();
        }
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.SelectNumberTestCases))]
    public void SelectNumber_Should_Return(JsonPathTestCaseData<decimal?> testCaseData)
    {
        // Arrange
        HttpResponseJson httpResponseJson = SampleJsonDocuments.BuildSample1();
        // Act
        decimal? selectedNumber = testCaseData.Extractor(httpResponseJson);
        // Assert
        if (testCaseData.ExpectedNbItems == 1)
        {
            selectedNumber.Should().Be(testCaseData.ExpectedResult);
        }
        else if (testCaseData.ExpectedNbItems == 0)
        {
            selectedNumber.Should().BeNull();
        }
        else
        {
            Assert.Fail("SelectNumber return at most one boolean!");
        }
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.SelectNumberArrayTestCases))]
    public void SelectNumberArray_Should_Return(JsonPathTestCaseData<decimal?[]?> testCaseData)
    {
        // Arrange
        HttpResponseJson httpResponseJson = SampleJsonDocuments.BuildSample1();
        // Act
        decimal?[]? selectedNumbers = testCaseData.Extractor(httpResponseJson);
        // Assert
        if (testCaseData.ExpectedNbItems > 0)
        {
            selectedNumbers.Should()
                .HaveCount(testCaseData.ExpectedNbItems)
                .And.ContainInOrder(testCaseData.ExpectedResult);
        }
        else
        {
            selectedNumbers.Should().BeNull();
        }
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.SelectStringTestCases))]
    public void SelectString_Should_Return(JsonPathTestCaseData<string?> testCaseData)
    {
        // Arrange
        HttpResponseJson httpResponseJson = SampleJsonDocuments.BuildSample1();
        // Act
        string? selectedString = testCaseData.Extractor(httpResponseJson);
        // Assert
        if (testCaseData.ExpectedNbItems == 1)
        {
            selectedString.Should().Be(testCaseData.ExpectedResult);
        }
        else if (testCaseData.ExpectedNbItems == 0)
        {
            selectedString.Should().BeNull();
        }
        else
        {
            Assert.Fail("SelectString return at most one string!");
        }
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.SelectStringArrayTestCases))]
    public void SelectStringArray_Should_Return(JsonPathTestCaseData<string?[]?> testCaseData)
    {
        // Arrange
        HttpResponseJson httpResponseJson = SampleJsonDocuments.BuildSample1();
        // Act
        string?[]? selectedStrings = testCaseData.Extractor(httpResponseJson);
        // Assert
        if (testCaseData.ExpectedNbItems > 0)
        {
            selectedStrings.Should()
                .HaveCount(testCaseData.ExpectedNbItems)
                .And.ContainInOrder(testCaseData.ExpectedResult);
        }
        else
        {
            selectedStrings.Should().BeNull();
        }
    }

    public class JsonPathTestCaseData<T>
    {
        public int ExpectedNbItems { get; init; } = 1;

        public T? ExpectedResult { get; init; }

        public Func<IJsonDocumentProvider, T?> Extractor { get; init; } = null!;
    }

    public static class SampleJsonDocuments
    {
        public static HttpResponseJson BuildSample1()
        {
            JsonObject jsonObject = new()
            {
                ["id"] = "P123456",
                ["status"] = "P",
                ["amount"] = 1234.5678m,
                ["quantity"] = 2345L,
                ["flag"] = true,
                ["children"] = new JsonArray(
                    new JsonObject
                    {
                        ["id"] = "C789012",
                        ["status"] = "C",
                        ["amount"] = 12.34m,
                        ["quantity"] = 6789L,
                        ["flag"] = false,
                    },
                    new JsonObject
                    {
                        ["id"] = "L345678",
                        ["status"] = "U",
                        ["amount"] = 56.78m,
                        ["quantity"] = 1234L,
                        ["flag"] = true,
                    },
                    new JsonObject
                    {
                        ["id"] = "L345678",
                        ["status"] = "C",
                        ["amount"] = 90.12m,
                        ["quantity"] = 5678L,
                        ["flag"] = false,
                    }
                ),
                ["friends"] = new JsonArray(
                    new JsonObject
                    {
                        ["id"] = "P789012",
                        ["status"] = "P",
                        ["amount"] = 9876.5432m,
                        ["quantity"] = 5432L,
                        ["flag"] = false,
                        ["children"] = new JsonArray(
                            new JsonObject
                            {
                                ["id"] = "C901234",
                                ["status"] = "C",
                                ["amount"] = 12.34m,
                                ["quantity"] = 1098L,
                                ["flag"] = false,
                            },
                            new JsonObject
                            {
                                ["id"] = "C567890",
                                ["status"] = "U",
                                ["amount"] = 56.78m,
                                ["quantity"] = 7654L,
                                ["flag"] = true,
                            }
                        )
                    }
                )
            };

            return jsonObject.ToHttpResponseJson();
        }
    }

    public static class TestCases
    {
        public static IEnumerable<TestCaseData> SelectBooleanArrayTestCases()
        {
            yield return new TestCaseData(new JsonPathTestCaseData<bool?[]?>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectBooleanArray("$.children[?(@.status=='V')].flag"),
                ExpectedResult = null,
                ExpectedNbItems = 0,
            }).SetName("Null_When_FilteredSelectorMatchedNoItem");
            yield return new TestCaseData(new JsonPathTestCaseData<bool?[]?>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectBooleanArray("$.children[?(@.status=='U')].flag"),
                ExpectedResult = new bool?[] { true },
                ExpectedNbItems = 1,
            }).SetName("BooleanArray_When_FilteredSelectorMatchedItem");
            yield return new TestCaseData(new JsonPathTestCaseData<bool?[]?>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectBooleanArray("$.children[?(@.status=='C')].flag"),
                ExpectedResult = new bool?[] { false, false },
                ExpectedNbItems = 2,
            }).SetName("BooleanArray_When_FilteredSelectorMatchedItems");
            yield return new TestCaseData(new JsonPathTestCaseData<bool?[]>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectBooleanArray("$.children[*].flag"),
                ExpectedResult = new bool?[] { false, true, false },
                ExpectedNbItems = 3,
            }).SetName("BooleanArray_When_UnfilteredSelectorMatchedItems");
        }

        public static IEnumerable<TestCaseData> SelectBooleanTestCases()
        {
            yield return new TestCaseData(new JsonPathTestCaseData<bool?>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectBoolean("$.flag"),
                ExpectedResult = true,
            }).SetName("Boolean_FromProperty");
            yield return new TestCaseData(new JsonPathTestCaseData<bool?>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectBoolean("$.friends[0].flag"),
                ExpectedResult = false,
            }).SetName("Boolean_FromArrayItemProperty");
        }

        public static IEnumerable<TestCaseData> SelectNumberArrayTestCases()
        {
            yield return new TestCaseData(new JsonPathTestCaseData<decimal?[]?>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectNumberArray("$.children[?(@.status=='V')].amount"),
                ExpectedResult = null,
                ExpectedNbItems = 0,
            }).SetName("Null_When_FilteredSelectorMatchedNoItem");
            yield return new TestCaseData(new JsonPathTestCaseData<decimal?[]?>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectNumberArray("$.children[?(@.status=='U')].amount"),
                ExpectedResult = new decimal?[] { 56.78m },
                ExpectedNbItems = 1,
            }).SetName("NumberArray_When_FilteredSelectorMatchedItem");
            yield return new TestCaseData(new JsonPathTestCaseData<decimal?[]?>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectNumberArray("$.children[?(@.status=='C')].amount"),
                ExpectedResult = new decimal?[] { 12.34m, 90.12m },
                ExpectedNbItems = 2,
            }).SetName("NumberArray_When_FilteredSelectorMatchedItems");
            yield return new TestCaseData(new JsonPathTestCaseData<decimal?[]>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectNumberArray("$.children[*].amount"),
                ExpectedResult = new decimal?[] { 12.34m, 56.78m, 90.12m },
                ExpectedNbItems = 3,
            }).SetName("NumberArray_When_UnfilteredSelectorMatchedItems");
        }

        public static IEnumerable<TestCaseData> SelectNumberTestCases()
        {
            yield return new TestCaseData(new JsonPathTestCaseData<decimal?>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectNumber("$.amount"),
                ExpectedResult = 1234.5678m,
            }).SetName("Number_FromProperty");
            yield return new TestCaseData(new JsonPathTestCaseData<decimal?>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectNumber("$.quantity"),
                ExpectedResult = 2345m,
            }).SetName("Number_FromPropertyWithoutDigit");
            yield return new TestCaseData(new JsonPathTestCaseData<decimal?>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectNumber("$.friends[0].amount"),
                ExpectedResult = 9876.5432m,
            }).SetName("Number_FromArrayItemProperty");
            yield return new TestCaseData(new JsonPathTestCaseData<decimal?>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectNumber("$.friends[0].quantity"),
                ExpectedResult = 5432m,
            }).SetName("Number_FromArrayItemPropertyWithoutDigit");
        }

        public static IEnumerable<TestCaseData> SelectStringArrayTestCases()
        {
            yield return new TestCaseData(new JsonPathTestCaseData<string?[]>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectStringArray("$.children[?(@.status=='V')].status"),
                ExpectedResult = null,
                ExpectedNbItems = 0,
            }).SetName("Null_When_FilteredSelectorMatchedNoItem");
            yield return new TestCaseData(new JsonPathTestCaseData<string?[]>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectStringArray("$.children[?(@.status=='U')].status"),
                ExpectedResult = new[] { "U" },
                ExpectedNbItems = 1,
            }).SetName("StringArray_When_FilteredSelectorMatchedItem");
            yield return new TestCaseData(new JsonPathTestCaseData<string?[]>
            {
                Extractor = jsonDocumentProvider =>
                    jsonDocumentProvider.SelectStringArray("$.children[?(@.status=='C')].status"),
                ExpectedResult = new[] { "C", "C" },
                ExpectedNbItems = 2,
            }).SetName("StringArray_When_FilteredSelectorMatchedItems");
            yield return new TestCaseData(new JsonPathTestCaseData<string?[]>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectStringArray("$.children[*].status"),
                ExpectedResult = new[] { "C", "U", "C" },
                ExpectedNbItems = 3,
            }).SetName("StringArray_When_UnfilteredSelectorMatchedItems");
        }

        public static IEnumerable<TestCaseData> SelectStringTestCases()
        {
            yield return new TestCaseData(new JsonPathTestCaseData<string?>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectString("$.id"),
                ExpectedResult = "P123456",
            }).SetName("String_FromProperty");
            yield return new TestCaseData(new JsonPathTestCaseData<string?>
            {
                Extractor = jsonDocumentProvider => jsonDocumentProvider.SelectString("$.friends[0].id"),
                ExpectedResult = "P789012",
            }).SetName("String_FromArrayItemProperty");
        }
    }
}