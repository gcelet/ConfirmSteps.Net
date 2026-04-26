namespace ConfirmSteps.Net.Tests.Reporting;

using AwesomeAssertions;
using AwesomeAssertions.Execution;

using ConfirmSteps.Reporting;

[TestFixture]
public class ScenarioExecutionSummaryReporterTests
{
  public enum AssertionType
  {
    Ignore,

    Contains,

    NotContains
  }

  private static ConfirmStepResult<TestData> BuildTestResult(
    string scenarioTitle = "Unit test scenario",
    string step1Title = "Step 1",
    string step2Title = "Step 2",
    string step3Title = "Step 3",
    ConfirmStatus confirmStatus = ConfirmStatus.Failure,
    bool haveResultException = true,
    string resultExceptionMessage = "Some result error",
    bool haveStepException = true,
    string stepExceptionMessage = "Some step error")
  {
    Exception? resultException = haveResultException ? new Exception(resultExceptionMessage) : null;
    Exception? stepException = haveStepException ? new Exception(stepExceptionMessage) : null;
    List<StepResult<TestData>> stepResults = new()
    {
      new StepResult<TestData>
      {
        Title = step1Title,
        Status = ConfirmStatus.Success,
        State = StepState.Done
      },
      new StepResult<TestData>
      {
        Title = step2Title,
        Status = haveStepException ? ConfirmStatus.Failure : ConfirmStatus.Success,
        State = StepState.Done,
        Exception = stepException
      },
      new StepResult<TestData>
      {
        Title = step3Title,
        Status = haveStepException ? ConfirmStatus.Indecisive : ConfirmStatus.Success,
        State = haveStepException ? StepState.Idle : StepState.Done
      }
    };

    TestData data = new();
    Dictionary<string, object> vars = new()
    {
      ["Var1"] = "Value1",
      ["Var2"] = 123,
      ["Var3"] = true
    };

    ConfirmStepResult<TestData> confirmStepResult = new(scenarioTitle, confirmStatus, stepResults, data, vars, resultException);

    return confirmStepResult;
  }

  private static void Verify(AssertionType assertionType, string source, string expected)
  {
    switch (assertionType)
    {
      case AssertionType.Contains:
      {
        source.Should().Contain(expected);
        break;
      }

      case AssertionType.NotContains:
      {
        source.Should().NotContain(expected);
        break;
      }
    }
  }

  private static void Verify(AssertionType assertionType, string source, string[] expected)
  {
    switch (assertionType)
    {
      case AssertionType.Contains:
      {
        source.Should().ContainAll(expected);
        break;
      }

      case AssertionType.NotContains:
      {
        source.Should().NotContainAll(expected);
        break;
      }
    }
  }


  [TestCaseSource(typeof(TestCases), nameof(TestCases.EnumerateTestCases))]
  public void GetExecutionSummary_Should_Return_Correctly(GetExecutionSummaryTestCase testCaseData)
  {
    // Arrange
    ConfirmStepResult<TestData> confirmStepResult = BuildTestResult();
    // Act
    string summary = confirmStepResult.GetExecutionSummary(testCaseData.Configure);
    // Assert
    using (new AssertionScope())
    {
      Verify(testCaseData.ResultSummaryAssertion, summary, testCaseData.ExpectedResultSummary);
      Verify(testCaseData.ResultExceptionAssertion, summary, testCaseData.ExpectedResultException);
      Verify(testCaseData.StepsSummaryAssertion, summary, testCaseData.ExpectedStepsSummary);
      Verify(testCaseData.StepsAssertion, summary, testCaseData.ExpectedSteps!);
      Verify(testCaseData.StepExceptionsAssertion, summary, testCaseData.ExpectedStepExceptions!);
      Verify(testCaseData.VarsAssertion, summary, testCaseData.ExpectedVars!);
    }
  }

  public class GetExecutionSummaryTestCase
  {
    public required Action<ScenarioExecutionSummaryOptions>? Configure { get; init; }

    public required string ExpectedResultException { get; init; }

    public required string ExpectedResultSummary { get; init; }

    public required string[]? ExpectedStepExceptions { get; init; }

    public required string[]? ExpectedSteps { get; init; }

    public required string ExpectedStepsSummary { get; init; }

    public required string[]? ExpectedVars { get; init; }

    public required AssertionType ResultExceptionAssertion { get; init; }

    public required AssertionType ResultSummaryAssertion { get; init; }

    public required AssertionType StepExceptionsAssertion { get; init; }

    public required AssertionType StepsAssertion { get; init; }

    public required AssertionType StepsSummaryAssertion { get; init; }

    public required AssertionType VarsAssertion { get; init; }
  }

  private static class TestCases
  {
    public static IEnumerable<TestCaseData> EnumerateTestCases()
    {
      yield return BuildTestCaseData("001-DefaultOptions",
        configure: null
      );

      yield return BuildTestCaseData("002-ExcludeResultSummary",
        configure: options => options.ExcludeResultSummary(),
        resultSummaryAssertion: AssertionType.NotContains,
        resultExceptionAssertion: AssertionType.NotContains
      );

      yield return BuildTestCaseData("003-ExcludeSteps",
        configure: options => options.ExcludeSteps(),
        stepsAssertion: AssertionType.NotContains,
        stepExceptionsAssertion: AssertionType.NotContains
      );

      yield return BuildTestCaseData("004-DefaultWithoutEmoticons",
        configure: options => options.UsePredefinedTemplate(ScenarioExecutionSummaryPredefinedTemplate.DefaultWithoutEmoticons),
        expectedResultSummary: "Scenario \"Unit test scenario\" — Status: Fail",
        stepsAssertion: AssertionType.Contains,
        expectedSteps:
        [
          "  ⁃1: [Pass] Step 1  (State: Done)",
          "  ⁃2: [Fail] Step 2  (State: Done)",
          "  ⁃3: [Skip] Step 3  (State: Idle)"
        ]
      );

      yield return BuildTestCaseData("005-ExcludePassedSteps",
        configure: options => options.ExcludePassedSteps(),
        stepsAssertion: AssertionType.Contains,
        expectedSteps:
        [
          "  ⁃2: [❌ Fail] Step 2  (State: ✅ Done)",
          "  ⁃3: [⏭️ Skip] Step 3  (State: ⏸️ Idle)"
        ]
      );

      yield return BuildTestCaseData("006-ExcludeVars",
        configure: options => options.ExcludeVars(),
        varsAssertion: AssertionType.NotContains
      );

      yield return BuildTestCaseData("007-CustomEmoticonsAndTexts",
        configure: options => options
          .UseEmoticonFor(ConfirmStatus.Success, "STAR")
          .UseTextFor(ConfirmStatus.Success, "WIN")
          .UseEmoticonFor(StepState.Done, "FINISH")
          .UseTextFor(StepState.Done, "COMPLETED"),
        stepsAssertion: AssertionType.Contains,
        expectedSteps:
        [
          "  ⁃1: [STAR WIN] Step 1  (State: FINISH COMPLETED)"
        ]
      );

      yield return BuildTestCaseData("008-CustomTemplates",
        configure: options => options
          .UseTemplateForResultSummary(t => $"Scenario: {t.ScenarioTitle} ({t.ScenarioStatusText})")
          .UseTemplateForStepSummary(t => $"#{t.StepNumber}: {t.StepTitle}")
          .ExcludeStepsSummary()
          .ExcludeVars(),
        resultSummaryAssertion: AssertionType.Contains,
        expectedResultSummary: "Scenario: Unit test scenario (Fail)",
        stepsAssertion: AssertionType.Contains,
        expectedSteps:
        [
          "#1: Step 1",
          "#2: Step 2",
          "#3: Step 3"
        ]
      );

      yield return BuildTestCaseData("009-ExcludeSkippedSteps",
        configure: options => options.ExcludeSkippedSteps(),
        stepsAssertion: AssertionType.Contains,
        expectedSteps:
        [
          "  ⁃1: [✅ Pass] Step 1  (State: ✅ Done)",
          "  ⁃2: [❌ Fail] Step 2  (State: ✅ Done)"
        ]
      );

      yield return BuildTestCaseData("010-ExcludeResultException",
        configure: options => options.ExcludeResultException(),
        resultExceptionAssertion: AssertionType.NotContains
      );

      yield return BuildTestCaseData("011-ExcludeStepException",
        configure: options => options.ExcludeStepException(),
        stepExceptionsAssertion: AssertionType.NotContains
      );

      yield return BuildTestCaseData("012-IncludeVars",
        configure: options => options.IncludeVars(),
        varsAssertion: AssertionType.Contains,
        expectedVars:
        [
          "Vars:",
          "  Var1: Value1",
          "  Var2: 123",
          "  Var3: True"
        ]
      );

      yield return BuildTestCaseData("013-IncludePassedSteps",
        configure: options => options.IncludePassedSteps(),
        stepsAssertion: AssertionType.Contains,
        expectedSteps:
        [
          "  ⁃1: [✅ Pass] Step 1  (State: ✅ Done)",
          "  ⁃2: [❌ Fail] Step 2  (State: ✅ Done)",
          "  ⁃3: [⏭️ Skip] Step 3  (State: ⏸️ Idle)"
        ]
      );

      yield return BuildTestCaseData("014-IncludeResultException",
        configure: options => options.IncludeResultException(),
        resultExceptionAssertion: AssertionType.Contains
      );

      yield return BuildTestCaseData("015-IncludeResultSummary",
        configure: options => options.IncludeResultSummary(),
        resultSummaryAssertion: AssertionType.Contains
      );

      yield return BuildTestCaseData("016-IncludeSkippedSteps",
        configure: options => options.IncludeSkippedSteps(),
        stepsAssertion: AssertionType.Contains,
        expectedSteps:
        [
          "  ⁃1: [✅ Pass] Step 1  (State: ✅ Done)",
          "  ⁃2: [❌ Fail] Step 2  (State: ✅ Done)",
          "  ⁃3: [⏭️ Skip] Step 3  (State: ⏸️ Idle)"
        ]
      );

      yield return BuildTestCaseData("017-IncludeStepException",
        configure: options => options.IncludeStepException(),
        stepExceptionsAssertion: AssertionType.Contains
      );

      yield return BuildTestCaseData("018-IncludeSteps",
        configure: options => options.IncludeSteps(),
        stepsAssertion: AssertionType.Contains
      );

      yield return BuildTestCaseData("019-IncludeStepsSummary",
        configure: options => options.IncludeStepsSummary(),
        stepsSummaryAssertion: AssertionType.Contains
      );

      yield return BuildTestCaseData("020-UseTemplateForResulException",
        configure: options => options.UseTemplateForResulException(t => $"Error: {t.ExceptionMessage}"),
        resultExceptionAssertion: AssertionType.Contains,
        expectedResultException: "Error: Some result error"
      );

      yield return BuildTestCaseData("021-UseTemplateForResultException",
        configure: options => options.UseTemplateForResultException("Error: {{ExceptionMessage}}"),
        resultExceptionAssertion: AssertionType.Contains,
        expectedResultException: "Error: Some result error"
      );

      yield return BuildTestCaseData("022-UseTemplateForResultSummary",
        configure: options => options.UseTemplateForResultSummary("Summary: {{ScenarioTitle}}"),
        resultSummaryAssertion: AssertionType.Contains,
        expectedResultSummary: "Summary: Unit test scenario"
      );

      yield return BuildTestCaseData("023-UseTemplateForStepException-TemplateString",
        configure: options => options.UseTemplateForStepException("StepError: {{ExceptionMessage}}"),
        stepExceptionsAssertion: AssertionType.Contains,
        expectedStepExceptions: ["StepError: Some step error"]
      );

      yield return BuildTestCaseData("024-UseTemplateForStepException-Factory",
        configure: options => options.UseTemplateForStepException(t => $"StepError: {t.ExceptionMessage}"),
        stepExceptionsAssertion: AssertionType.Contains,
        expectedStepExceptions: ["StepError: Some step error"]
      );

      yield return BuildTestCaseData("025-UseTemplateForStepsSummary-TemplateString",
        configure: options => options.UseTemplateForStepsSummary("Summary: {{StepCount}} steps"),
        stepsSummaryAssertion: AssertionType.Contains,
        expectedStepsSummary: "Summary: 3 steps"
      );

      yield return BuildTestCaseData("026-UseTemplateForStepsSummary-Factory",
        configure: options => options.UseTemplateForStepsSummary(t => $"Summary: {t.StepCount} steps"),
        stepsSummaryAssertion: AssertionType.Contains,
        expectedStepsSummary: "Summary: 3 steps"
      );

      yield return BuildTestCaseData("027-UseTemplateForStepSummary-TemplateString",
        configure: options => options.UseTemplateForStepSummary("Step #{{StepNumber}}"),
        stepsAssertion: AssertionType.Contains,
        expectedSteps: ["Step #1", "Step #2", "Step #3"]
      );

      yield return BuildTestCaseData("028-UseTemplateForVar-TemplateString",
        configure: options => options.IncludeVars().UseTemplateForVar("Var {{VarName}}={{VarValue}}"),
        varsAssertion: AssertionType.Contains,
        expectedVars: ["Var Var1=Value1", "Var Var2=123", "Var Var3=True"]
      );

      yield return BuildTestCaseData("029-UseTemplateForVar-Factory",
        configure: options => options.IncludeVars().UseTemplateForVar(t => $"Var {t.VarName}={t.VarValue}"),
        varsAssertion: AssertionType.Contains,
        expectedVars: ["Var Var1=Value1", "Var Var2=123", "Var Var3=True"]
      );

      yield return BuildTestCaseData("030-UseTemplateForVars-TemplateString",
        configure: options => options.IncludeVars().UseTemplateForVars("Variables:"),
        varsAssertion: AssertionType.Contains,
        expectedVars: ["Variables:"]
      );

      yield return BuildTestCaseData("031-UseTemplateForVars-Factory",
        configure: options => options.IncludeVars().UseTemplateForVars(_ => "Variables list:"),
        varsAssertion: AssertionType.Contains,
        expectedVars: ["Variables list:"]
      );
    }

    private static TestCaseData BuildTestCaseData(string testName,
      Action<ScenarioExecutionSummaryOptions>? configure,
      AssertionType resultSummaryAssertion = AssertionType.Contains,
      string expectedResultSummary = "Scenario \"Unit test scenario\" — Status: ❌ Fail",
      AssertionType resultExceptionAssertion = AssertionType.Contains,
      string expectedResultException = "Exception: Some result error",
      AssertionType stepsSummaryAssertion = AssertionType.Ignore,
      string expectedStepsSummary = "Steps: [1/3]",
      AssertionType stepsAssertion = AssertionType.Ignore,
      string[]? expectedSteps = null,
      AssertionType stepExceptionsAssertion = AssertionType.Ignore,
      string[]? expectedStepExceptions = null,
      AssertionType varsAssertion = AssertionType.Ignore,
      string[]? expectedVars = null)
    {
      GetExecutionSummaryTestCase testCaseData = new()
      {
        Configure = configure,
        ResultSummaryAssertion = resultSummaryAssertion,
        ExpectedResultSummary = expectedResultSummary,
        ResultExceptionAssertion = resultExceptionAssertion,
        ExpectedResultException = expectedResultException,
        StepsSummaryAssertion = stepsSummaryAssertion,
        ExpectedStepsSummary = expectedStepsSummary,
        StepsAssertion = stepsAssertion,
        ExpectedSteps = expectedSteps ??
        [
          "  ⁃1: [✅ Pass] Step 1  (State: ✅ Done)",
          "  ⁃2: [❌ Fail] Step 2  (State: ✅ Done)",
          "  ⁃3: [⏭️ Skip] Step 3  (State: ⏸️ Idle)"
        ],
        StepExceptionsAssertion = stepExceptionsAssertion,
        ExpectedStepExceptions = expectedStepExceptions ??
        [
          "      Exception: Some step error"
        ],
        VarsAssertion = varsAssertion,
        ExpectedVars = expectedVars ??
        [
          "Vars:",
          "  Var1: Value1",
          "  Var2: 123",
          "  Var3: True"
        ]
      };

      return new TestCaseData(testCaseData)
          .SetName(testName)
        ;
    }
  }

  private class TestData
  {
  }
}
