namespace ConfirmSteps.Net.Tests;

using AwesomeAssertions;
using AwesomeAssertions.Execution;

using ConfirmSteps.Net.Tests.Sql;
using ConfirmSteps.Steps;
using ConfirmSteps.Steps.Sql;
using ConfirmSteps.Steps.Sql.CommandBuilding;
using ConfirmSteps.Steps.Sql.ResultParsing;
using ConfirmSteps.Templating;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using static CancellationExtensions;

[TestFixture]
public class SqlStepTests : SqlStepTestBase
{
    private static readonly Func<SqlCommandBuilder> SimpleCommandBuilder = () => SqlCommandBuilder.Query("SELECT 1");

    private static IEnumerable<TestCaseData> ArgumentNullGuardTestData()
    {
        List<Action<SqlResultSet, StepContext<SqlStepScenarioData>>> verifiers = new();
        List<ISqlResultSetExtractor<SqlStepScenarioData>> extractors = new();

        yield return new TestCaseData(
                (Action)(() => _ = new SqlStep<SqlStepScenarioData>("title", null!, verifiers,
                    StepVerificationMode.StopOnFirstFailure, extractors)),
                "commandBuilder")
            .SetName("SqlStep_Constructor_Should_Throw_WhenCommandBuilderIsNull");

        yield return new TestCaseData(
                (Action)(() => _ = new SqlStep<SqlStepScenarioData>("title", SimpleCommandBuilder, null!,
                    StepVerificationMode.StopOnFirstFailure, extractors)),
                "verifiers")
            .SetName("SqlStep_Constructor_Should_Throw_WhenVerifiersIsNull");

        yield return new TestCaseData(
                (Action)(() => _ = new SqlStep<SqlStepScenarioData>("title", SimpleCommandBuilder, verifiers,
                    StepVerificationMode.StopOnFirstFailure, null!)),
                "extractors")
            .SetName("SqlStep_Constructor_Should_Throw_WhenExtractorsIsNull");

        yield return new TestCaseData(
                (Action)(() => _ = new SqlStepBuilder<SqlStepScenarioData>(null!, SimpleCommandBuilder)),
                "title")
            .SetName("SqlStepBuilder_Constructor_Should_Throw_WhenTitleIsNull");

        yield return new TestCaseData(
                (Action)(() => _ = new SqlStepBuilder<SqlStepScenarioData>("title", null!)),
                "commandBuilder")
            .SetName("SqlStepBuilder_Constructor_Should_Throw_WhenCommandBuilderIsNull");

        yield return new TestCaseData(
                (Action)(() => new SqlStepBuilder<SqlStepScenarioData>("title", SimpleCommandBuilder)
                    .VerifyRows(null!)),
                "verify")
            .SetName("SqlStepBuilder_VerifyRows_Should_Throw_WhenVerifyIsNull");

        yield return new TestCaseData(
                (Action)(() => SqlStepExtensions.SqlStep<SqlStepScenarioData>(
                    null!, "title", SimpleCommandBuilder, _ => { })),
                "stepBuilderAppender")
            .SetName("SqlStepExtensions_SqlStep_Should_Throw_WhenStepBuilderAppenderIsNull");

        yield return new TestCaseData(
                (Action)(() => Scenario.New<SqlStepScenarioData>("[Guard]")
                    .WithSteps(steps => steps.SqlStep("title", SimpleCommandBuilder, null!))),
                "stepBuilder")
            .SetName("SqlStepExtensions_SqlStep_Should_Throw_WhenStepBuilderIsNull");

        yield return new TestCaseData(
                (Action)(() => new ServiceCollection().AddExternalDbProviderFactory(null!, "cs")),
                "factory")
            .SetName("AddExternalDbProviderFactory_Should_Throw_WhenFactoryIsNull");

        yield return new TestCaseData(
                (Action)(() => new ServiceCollection()
                    .AddExternalDbProviderFactory(SqliteFactory.Instance, null!)),
                "connectionString")
            .SetName("AddExternalDbProviderFactory_Should_Throw_WhenConnectionStringIsNull");

        yield return new TestCaseData(
                (Action)(() => ((IServiceCollection)null!)
                    .AddExternalDbProviderFactory(SqliteFactory.Instance, "cs")),
                "services")
            .SetName("AddExternalDbProviderFactory_Should_Throw_WhenServicesIsNull");

        yield return new TestCaseData(
                (Action)(() => _ = new ExternalDbProviderFactoryProvider(null!, "cs")),
                "factory")
            .SetName("ExternalDbProviderFactoryProvider_Constructor_Should_Throw_WhenFactoryIsNull");

        yield return new TestCaseData(
                (Action)(() => _ = new ExternalDbProviderFactoryProvider(SqliteFactory.Instance, null!)),
                "connectionString")
            .SetName("ExternalDbProviderFactoryProvider_Constructor_Should_Throw_WhenConnectionStringIsNull");
    }

    [Test]
    public async Task SingleSqlStepScenario_Should_ReturnMatchingRows_WhenQueryIsParameterized()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE Orders (Id INTEGER PRIMARY KEY, CustomerId INTEGER, Status TEXT)");
        ExecuteSetupSql("INSERT INTO Orders (Id, CustomerId, Status) VALUES (1, 42, 'Open')");
        ExecuteSetupSql("INSERT INTO Orders (Id, CustomerId, Status) VALUES (2, 42, 'Closed')");
        ExecuteSetupSql("INSERT INTO Orders (Id, CustomerId, Status) VALUES (3, 7, 'Open')");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepReturnMatchingRows]")
                .WithGlobals(g => g.UseConst("customerId", 42))
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-Orders",
                        () => SqlCommandBuilder.Query("SELECT Id, Status FROM Orders WHERE CustomerId = @customerId")
                            .WithParameter("customerId", "{{customerId}}"),
                        step => step.VerifyRows((resultSet, _) => resultSet.RowCount.Should().Be(2))
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Should().BeEquivalentTo(new
        {
            Status = ConfirmStatus.Success,
            Exception = (Exception)null!,
        });
    }

    /// <summary>
    /// <see cref="SqlStepBuilder{T}.VerifyRows"/> can be chained several times; nothing else pins down
    /// that the resulting verifiers actually run in the order they were declared rather than, say,
    /// reversed or unspecified order.
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_RunVerifyRowsInDeclarationOrder()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE T (Id INTEGER)");
        ExecuteSetupSql("INSERT INTO T (Id) VALUES (1)");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepRunVerifyRowsInOrder]")
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-T",
                        () => SqlCommandBuilder.Query("SELECT Id FROM T"),
                        step => step
                            .VerifyRows((_, ctx) => ctx.Vars["order"] = "first")
                            .VerifyRows((_, ctx) => ctx.Vars["order"] = (string)ctx.Vars["order"] + ",second")
                            .VerifyRows((_, ctx) => ctx.Vars["order"] = (string)ctx.Vars["order"] + ",third")
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Vars["order"].Should().Be("first,second,third");
    }

    /// <summary>
    /// Pins the default <see cref="StepVerificationMode.StopOnFirstFailure"/> behavior: the first verifier to
    /// throw stops the chain immediately and its own exception surfaces as-is, with no aggregation - mirroring
    /// HttpStepBuilder's default. See
    /// <see cref="SingleSqlStepScenario_Should_RunAllVerifiersWhenUsingVerifyAllModeEvenIfSomeVerifiersFail"/>
    /// for the opposite, opt-in behavior.
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_StopAtFirstFailingVerifier_WithNoAggregation()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE T (Id INTEGER)");
        ExecuteSetupSql("INSERT INTO T (Id) VALUES (1)");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepStopAtFirstFailingVerifier]")
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-T",
                        () => SqlCommandBuilder.Query("SELECT Id FROM T"),
                        step => step
                            .VerifyRows((_, _) => throw new InvalidOperationException("first verifier failed"))
                            .VerifyRows((_, ctx) => ctx.Vars["secondRan"] = true)
                            .VerifyRows((_, ctx) => ctx.Vars["thirdRan"] = true)
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        using AssertionScope scope = new();
        confirmResult.Status.Should().Be(ConfirmStatus.Failure);
        confirmResult.Exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("first verifier failed");
        confirmResult.Vars.Should().NotContainKey("secondRan");
        confirmResult.Vars.Should().NotContainKey("thirdRan");
    }

    /// <summary>
    /// Under <see cref="StepVerificationMode.VerifyAll"/>, every verifier runs regardless of earlier failures and
    /// their exceptions are combined into an <see cref="AggregateException"/> - mirroring HttpStepBuilder's
    /// VerifyAll mode (<c>SingleGetStepScenario_Should_RunAllVerifiersWhenUsingVerifyAllModeEvenIfSomeVerifiersFail</c>
    /// in <c>HttpStepTests</c>).
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_RunAllVerifiersWhenUsingVerifyAllModeEvenIfSomeVerifiersFail()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE T (Id INTEGER)");
        ExecuteSetupSql("INSERT INTO T (Id) VALUES (1)");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepRunAllVerifiersWhenUsingVerifyAllMode]")
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-T",
                        () => SqlCommandBuilder.Query("SELECT Id FROM T"),
                        step => step
                            .WithVerificationMode(StepVerificationMode.VerifyAll)
                            .VerifyRows((_, _) => throw new Exception("first verifier failed"))
                            .VerifyRows((_, _) => throw new Exception("second verifier failed"))
                            .VerifyRows((_, _) => throw new Exception("third verifier failed"))
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        using AssertionScope scope = new();
        confirmResult.Status.Should().Be(ConfirmStatus.Failure);
        confirmResult.Exception.Should().BeOfType<AggregateException>()
            .Subject.InnerExceptions.Should().HaveCount(3)
            .And.SatisfyRespectively(
                first => first.Message.Should().Be("first verifier failed"),
                second => second.Message.Should().Be("second verifier failed"),
                third => third.Message.Should().Be("third verifier failed")
            );
    }

    /// <summary>
    /// <c>SqlCommandBuilder</c>'s factory delegate is re-invoked on every execution specifically so that
    /// the command text and parameter values can depend on scenario vars. This pins that contract down
    /// end-to-end, through both the command text and a parameter value at once.
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_RenderCommandTextAndParametersFromScenarioVars()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE Orders (Id INTEGER PRIMARY KEY, Status TEXT)");
        ExecuteSetupSql("INSERT INTO Orders (Id, Status) VALUES (1, 'Open')");
        ExecuteSetupSql("INSERT INTO Orders (Id, Status) VALUES (2, 'Closed')");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepRenderFromVars]")
                .WithGlobals(g => g.UseConst("status", "Closed").UseConst("label", "matched"))
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-Orders",
                        () => SqlCommandBuilder
                            .Query("SELECT Id, '{{label}}' AS Label FROM Orders WHERE Status = @status")
                            .WithParameter("status", "{{status}}"),
                        step => step.VerifyRows((resultSet, _) =>
                        {
                            resultSet.RowCount.Should().Be(1);
                            resultSet.Value(0, "Id").Should().Be(2L);
                            resultSet.Value(0, "Label").Should().Be("matched");
                        })
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Status.Should().Be(ConfirmStatus.Success);
    }

    /// <summary>
    /// A real SQL failure must surface as the actual exception, not be swallowed or transformed - and
    /// because Execute did not succeed, Verify must never run (proven by the marker variable staying unset).
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_Fail_WhenSqlExecutionFails_AndNeverRunVerify()
    {
        // Arrange
        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepFailOnSqlError]")
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-Missing",
                        () => SqlCommandBuilder.Query("SELECT * FROM DoesNotExist"),
                        step => step.VerifyRows((_, ctx) => ctx.Vars["verifyRan"] = true)
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        using AssertionScope scope = new();
        confirmResult.Status.Should().Be(ConfirmStatus.Failure);
        confirmResult.Exception.Should().BeOfType<SqliteException>();
        confirmResult.Vars.Should().NotContainKey("verifyRan");
    }

    /// <summary>
    /// Forgetting to register the DB provider is the wiring mistake every consumer will make once; the
    /// failure it produces should stay a clear scenario failure rather than an unrelated crash.
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_Fail_WhenDbProviderFactoryProviderIsNotRegistered()
    {
        // Arrange
        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepFailWhenProviderNotRegistered]")
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-1",
                        () => SqlCommandBuilder.Query("SELECT 1"),
                        step => step.VerifyRows((_, _) => { })
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        using AssertionScope scope = new();
        confirmResult.Status.Should().Be(ConfirmStatus.Failure);
        confirmResult.Exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain(nameof(IDbProviderFactoryProvider));
    }

    [Test]
    public async Task SingleSqlStepScenario_Should_Fail_WhenFactoryCreatesNoConnection()
    {
        // Arrange
        NullReturningDbProviderFactory factory = new() { ReturnNullConnection = true };

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepFailWhenFactoryCreatesNoConnection]")
                .WithServices(s => s.AddExternalDbProviderFactory(factory, ConnectionString))
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-1",
                        () => SqlCommandBuilder.Query("SELECT 1"),
                        step => step.VerifyRows((_, _) => { })
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Status.Should().Be(ConfirmStatus.Failure);
        confirmResult.Exception.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("the ADO.NET factory created no connection");
    }

    /// <summary>
    /// A missing variable must fail the step in Prepare, before Execute ever runs - proven here by deliberately
    /// NOT registering <see cref="IDbProviderFactoryProvider"/>: if Execute ran regardless, it would fail with
    /// a different, DI-resolution exception (as pinned by
    /// <see cref="SingleSqlStepScenario_Should_Fail_WhenDbProviderFactoryProviderIsNotRegistered"/>), not this one.
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_FailInPrepare_WhenCommandTextExpectsAVariableThatHasNoValue()
    {
        // Arrange
        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepFailInPrepareWhenVariableMissing]")
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-Missing",
                        () => SqlCommandBuilder.Query("SELECT * FROM T WHERE Id = {{missingId}}"),
                        step => step.VerifyRows((_, _) => { })
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        using AssertionScope scope = new();
        confirmResult.Status.Should().Be(ConfirmStatus.Failure);
        confirmResult.Exception.Should().BeOfType<UnresolvedTemplateVariableException>()
            .Which.Unresolved.Should().SatisfyRespectively(
                unresolved =>
                {
                    unresolved.Name.Should().Be("missingId");
                    unresolved.Location.Should().Be("command text");
                });
    }

    /// <summary>
    /// Every unresolved variable is reported in one shot, wherever it comes from - the command text and each
    /// input parameter - mirroring <c>EveryMissingVariableShouldBeReportedTogetherWithWhereItWasExpected</c> for HTTP.
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_ReportEveryUnresolvedVariableTogether_ForCommandTextAndEachParameter()
    {
        // Arrange
        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepReportEveryUnresolvedVariableTogether]")
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-Missing",
                        () => SqlCommandBuilder.Query("SELECT * FROM T WHERE Id = {{missingInText}}")
                            .WithParameter("p", "{{missingInParam}}"),
                        step => step.VerifyRows((_, _) => { })
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Exception.Should().BeOfType<UnresolvedTemplateVariableException>()
            .Which.Unresolved.Should().SatisfyRespectively(
                inText =>
                {
                    inText.Name.Should().Be("missingInText");
                    inText.Location.Should().Be("command text");
                },
                inParam =>
                {
                    inParam.Name.Should().Be("missingInParam");
                    inParam.Location.Should().Be("parameter 'p'");
                });
    }

    /// <summary>
    /// A variable bound to <c>null</c> counts as missing, matching how <see cref="TemplateString.Render"/> already
    /// treats it.
    /// </summary>
    [Test]
    public async Task AVariablePresentButNullShouldCountAsMissing_ForSqlCommand()
    {
        // Arrange
        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepNullVariableCountsAsMissing]")
                .WithGlobals(g => g.UseObject("id", _ => null!))
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-Missing",
                        () => SqlCommandBuilder.Query("SELECT * FROM T WHERE Id = {{id}}"),
                        step => step.VerifyRows((_, _) => { })
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Exception.Should().BeOfType<UnresolvedTemplateVariableException>()
            .Which.Unresolved.Should().ContainSingle(u => u.Name == "id");
    }

    /// <summary>
    /// An empty string is a value, not a missing one - the request proceeds, matching
    /// <c>AnEmptyValueShouldNotBlockTheRequest</c> for HTTP.
    /// </summary>
    [Test]
    public async Task AnEmptyValueShouldNotBlockTheSqlCommand()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE T (Id INTEGER, Name TEXT)");
        ExecuteSetupSql("INSERT INTO T (Id, Name) VALUES (1, '')");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepEmptyValueDoesNotBlock]")
                .WithGlobals(g => g.UseConst("name", string.Empty))
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-T",
                        () => SqlCommandBuilder.Query("SELECT Id FROM T WHERE Name = @name")
                            .WithParameter("name", "{{name}}"),
                        step => step.VerifyRows((resultSet, _) => resultSet.RowCount.Should().Be(1))
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Status.Should().Be(ConfirmStatus.Success);
    }

    [Test]
    public async Task SingleSqlStepScenario_Should_ExtractToVars_FromResultSet()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE T (Id INTEGER)");
        ExecuteSetupSql("INSERT INTO T (Id) VALUES (1)");
        ExecuteSetupSql("INSERT INTO T (Id) VALUES (2)");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepExtractToVars]")
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-T",
                        () => SqlCommandBuilder.Query("SELECT COUNT(*) AS Cnt FROM T"),
                        step => step.Extract(extract =>
                            extract.ToVars("rowCount", resultSet => resultSet.Value(0, "Cnt")))
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Status.Should().Be(ConfirmStatus.Success);
        confirmResult.Vars["rowCount"].Should().Be(2L);
    }

    [Test]
    public async Task SingleSqlStepScenario_Should_ExtractToDataProperty_FromResultSet()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE T (Id INTEGER)");
        ExecuteSetupSql("INSERT INTO T (Id) VALUES (1)");
        ExecuteSetupSql("INSERT INTO T (Id) VALUES (2)");
        ExecuteSetupSql("INSERT INTO T (Id) VALUES (3)");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepExtractToDataProperty]")
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-T",
                        () => SqlCommandBuilder.Query("SELECT COUNT(*) AS Cnt FROM T"),
                        step => step.Extract(extract =>
                            extract.ToData(d => d.RowCount, resultSet => resultSet.Value(0, "Cnt")))
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Status.Should().Be(ConfirmStatus.Success);
        confirmResult.Data.RowCount.Should().Be(3L);
    }

    /// <summary>
    /// Mirrors HTTP's extraction rule: when the extractor delegate returns null, nothing is written - a step
    /// can extract optimistically without clobbering a value a previous step already set.
    /// </summary>
    [Test]
    public async Task SingleSqlStepScenario_Should_NotWriteVar_WhenExtractorReturnsNull()
    {
        // Arrange
        ExecuteSetupSql("CREATE TABLE T (Id INTEGER)");

        Scenario<SqlStepScenarioData> scenario = Scenario
                .New<SqlStepScenarioData>("[SqlStepExtractorReturningNullWritesNothing]")
                .WithGlobals(g => g.UseConst("preset", "untouched"))
                .WithServices(RegisterSqlite)
                .WithSteps(steps => steps
                    .SqlStep<SqlStepScenarioData>("[Step-01]-SELECT-T",
                        () => SqlCommandBuilder.Query("SELECT COUNT(*) AS Cnt FROM T"),
                        step => step.Extract(extract =>
                            extract.ToVars("preset", _ => null))
                    )
                )
                .Build()
            ;

        SqlStepScenarioData data = new();

        // Act
        using CancellationTokenSource cts = CreateDefaultScenarioCancellationTokenSource();
        ConfirmStepResult<SqlStepScenarioData> confirmResult = await scenario.ConfirmSteps(data, cts.Token);

        // Assert
        confirmResult.Status.Should().Be(ConfirmStatus.Success);
        confirmResult.Vars["preset"].Should().Be("untouched");
    }

    [TestCaseSource(nameof(ArgumentNullGuardTestData))]
    public void PublicEntryPoint_Should_ThrowArgumentNullException_ForRequiredParameter(Action act,
        string expectedParameterName)
    {
        act.Should().Throw<ArgumentNullException>().WithParameterName(expectedParameterName);
    }

    public class SqlStepScenarioData
    {
        public long RowCount { get; set; }
    }
}
