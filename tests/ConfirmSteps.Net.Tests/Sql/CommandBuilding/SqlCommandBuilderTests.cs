namespace ConfirmSteps.Net.Tests.Sql.CommandBuilding;

using System.Data;
using System.Data.Common;

using AwesomeAssertions;
using AwesomeAssertions.Execution;

using ConfirmSteps.Steps.Sql.CommandBuilding;
using ConfirmSteps.Templating;

using Microsoft.Data.Sqlite;

[TestFixture]
public class SqlCommandBuilderTests
{
    private SqliteConnection connection = null!;

    [SetUp]
    public void SetUp()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
    }

    [TearDown]
    public void TearDown()
    {
        connection.Dispose();
    }

    [Test]
    public void Build_Should_RenderCommandTextFromVars()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT {{value}} AS Value");
        Dictionary<string, object> vars = new(StringComparer.Ordinal) { ["value"] = "'hello'" };

        // Act
        using DbCommand command = builder.Build(SqliteFactory.Instance, connection, vars);

        // Assert
        command.CommandText.Should().Be("SELECT 'hello' AS Value");
    }

    /// <summary>
    /// Parameter values are always rendered as a string (<see cref="ConfirmSteps.Templating.TemplateString.Render"/>
    /// returns a string, never a typed value). This pins down that the design choice does not silently break a
    /// numeric comparison, thanks to SQLite's type affinity - not that the CLR type is "string" in isolation.
    /// </summary>
    [Test]
    public async Task Build_Should_RenderParameterValueAsStringAndLetSqliteAffinityMatchIt()
    {
        // Arrange
        using (DbCommand createTable = connection.CreateCommand())
        {
            createTable.CommandText = "CREATE TABLE T (Id INTEGER)";
            await createTable.ExecuteNonQueryAsync();
        }

        using (DbCommand insert = connection.CreateCommand())
        {
            insert.CommandText = "INSERT INTO T (Id) VALUES (42)";
            await insert.ExecuteNonQueryAsync();
        }

        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT COUNT(*) FROM T WHERE Id = @id")
            .WithParameter("id", "{{id}}");
        Dictionary<string, object> vars = new(StringComparer.Ordinal) { ["id"] = 42 };

        // Act
        using DbCommand command = builder.Build(SqliteFactory.Instance, connection, vars);
        object? matchCount = await command.ExecuteScalarAsync();

        // Assert
        matchCount.Should().Be(1L);
    }

    /// <summary>
    /// Re-declaring a parameter with the same name is meant to replace it, not accumulate a duplicate -
    /// the dictionary-backed storage makes this the natural behavior, worth pinning explicitly.
    /// </summary>
    [Test]
    public void WithParameter_Should_OverwritePreviousValue_WhenCalledTwiceWithSameName()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT @p")
            .WithParameter("p", "first")
            .WithParameter("p", "second");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        using DbCommand command = builder.Build(SqliteFactory.Instance, connection, vars);

        // Assert
        command.Parameters.Count.Should().Be(1);
        command.Parameters[0].Should().BeAssignableTo<DbParameter>().Which.Value.Should().Be("second");
    }

    [Test]
    public void Build_Should_ThrowArgumentNullException_WhenFactoryIsNull()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        Action act = () => builder.Build(null!, connection, vars);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("factory");
    }

    [Test]
    public void Build_Should_ThrowArgumentNullException_WhenConnectionIsNull()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        Action act = () => builder.Build(SqliteFactory.Instance, null!, vars);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("connection");
    }

    [Test]
    public void Build_Should_ThrowArgumentNullException_WhenVarsIsNull()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");

        // Act
        Action act = () => builder.Build(SqliteFactory.Instance, connection, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("vars");
    }

    [Test]
    public void WithParameter_Should_ThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");

        // Act
        Action act = () => builder.WithParameter(null!, "value");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("name");
    }

    [Test]
    public void Build_Should_ThrowInvalidOperationException_WhenFactoryCreatesNoCommand()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);
        NullReturningDbProviderFactory factory = new() { ReturnNullCommand = true };

        // Act
        Action act = () => builder.Build(factory, connection, vars);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("the ADO.NET factory created no command");
    }

    /// <summary>
    /// The half-built command must not leak when parameter creation fails - the try/catch/dispose in
    /// SqlCommandBuilder.Build is only worth having if it actually runs before the exception is observed.
    /// </summary>
    [Test]
    public void Build_Should_ThrowInvalidOperationException_WhenFactoryCreatesNoParameter()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT @p").WithParameter("p", "value");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);
        NullReturningDbProviderFactory factory = new() { ReturnNullParameter = true };

        // Act
        Action act = () => builder.Build(factory, connection, vars);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("the ADO.NET factory created no parameter");
    }

    [Test]
    public void Query_Should_SetCommandTypeToText()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        using DbCommand command = builder.Build(SqliteFactory.Instance, connection, vars);

        // Assert
        command.CommandType.Should().Be(CommandType.Text);
    }

    /// <summary>
    /// SQLite has no stored procedures - Microsoft.Data.Sqlite's own command type refuses
    /// <see cref="CommandType.StoredProcedure"/> the instant it is set, so a <see cref="FakeDbProviderFactory"/>
    /// stands in here. This only pins that the right ADO.NET flag is positioned on the built command; it is not
    /// a claim that SQLite (or any particular provider) can execute one.
    /// </summary>
    [Test]
    public void StoredProcedure_Should_SetCommandTypeToStoredProcedureAndCommandTextToProcedureName()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.StoredProcedure("MyProc");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        using DbCommand command = builder.Build(FakeDbProviderFactory.Instance, connection, vars);

        // Assert
        using AssertionScope scope = new();
        command.CommandType.Should().Be(CommandType.StoredProcedure);
        command.CommandText.Should().Be("MyProc");
    }

    /// <summary>
    /// Microsoft.Data.Sqlite's own parameter type refuses any direction but Input the instant it is set, so a
    /// <see cref="FakeDbProviderFactory"/> stands in here.
    /// </summary>
    [Test]
    public void WithParameter_WithDirection_Should_SetDirectionAndRenderValue_OnBuiltCommand()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT @p")
            .WithParameter("p", "{{value}}", ParameterDirection.InputOutput);
        Dictionary<string, object> vars = new(StringComparer.Ordinal) { ["value"] = "hello" };

        // Act
        using DbCommand command = builder.Build(FakeDbProviderFactory.Instance, connection, vars);
        DbParameter parameter = command.Parameters[0].Should().BeAssignableTo<DbParameter>().Which;

        // Assert
        using AssertionScope scope = new();
        parameter.Direction.Should().Be(ParameterDirection.InputOutput);
        parameter.Value.Should().Be("hello");
    }

    [TestCase(ParameterDirection.Output)]
    [TestCase(ParameterDirection.ReturnValue)]
    public void WithParameter_WithDirection_Should_ThrowArgumentOutOfRangeException_WhenDirectionHasNoInputValue(
        ParameterDirection direction)
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT @p");

        // Act
        Action act = () => builder.WithParameter("p", "value", direction);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("direction");
    }

    [Test]
    public void WithOutputParameter_Should_SetDirectionToOutput_OnBuiltCommand()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.StoredProcedure("MyProc").WithOutputParameter("result");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        using DbCommand command = builder.Build(FakeDbProviderFactory.Instance, connection, vars);
        DbParameter parameter = command.Parameters[0].Should().BeAssignableTo<DbParameter>().Which;

        // Assert
        parameter.Direction.Should().Be(ParameterDirection.Output);
    }

    [Test]
    public void WithOutputParameter_Should_SetDirectionToReturnValue_WhenDirectionIsExplicit()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.StoredProcedure("MyProc")
            .WithOutputParameter("result", ParameterDirection.ReturnValue);
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        using DbCommand command = builder.Build(FakeDbProviderFactory.Instance, connection, vars);
        DbParameter parameter = command.Parameters[0].Should().BeAssignableTo<DbParameter>().Which;

        // Assert
        parameter.Direction.Should().Be(ParameterDirection.ReturnValue);
    }

    /// <summary>
    /// An output-only parameter has no input value to render; DBNull is the neutral value a caller reads back
    /// against if the command never actually populates it (e.g. against a provider with no real support for it).
    /// </summary>
    [Test]
    public void WithOutputParameter_Should_SetValueToDbNull_WhenBuilt()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.StoredProcedure("MyProc").WithOutputParameter("result");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        using DbCommand command = builder.Build(FakeDbProviderFactory.Instance, connection, vars);
        DbParameter parameter = command.Parameters[0].Should().BeAssignableTo<DbParameter>().Which;

        // Assert
        parameter.Value.Should().Be(DBNull.Value);
    }

    [TestCase(ParameterDirection.Input)]
    [TestCase(ParameterDirection.InputOutput)]
    public void WithOutputParameter_Should_ThrowArgumentOutOfRangeException_WhenDirectionNeedsAnInputValue(
        ParameterDirection direction)
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");

        // Act
        Action act = () => builder.WithOutputParameter("p", direction);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("direction");
    }

    [Test]
    public void WithOutputParameter_Should_ThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");

        // Act
        Action act = () => builder.WithOutputParameter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("name");
    }

    [Test]
    public void EnsureEveryVariableResolved_Should_Throw_WhenCommandTextExpectsAVariableThatHasNoValue()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT {{missing}}");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        Action act = () => builder.EnsureEveryVariableResolved(vars);

        // Assert
        act.Should().Throw<UnresolvedTemplateVariableException>()
            .Which.Unresolved.Should().ContainSingle(u => u.Name == "missing" && u.Location == "command text");
    }

    [Test]
    public void EnsureEveryVariableResolved_Should_Throw_WhenAnInputParameterExpectsAVariableThatHasNoValue()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1").WithParameter("p", "{{missing}}");
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        Action act = () => builder.EnsureEveryVariableResolved(vars);

        // Assert
        act.Should().Throw<UnresolvedTemplateVariableException>()
            .Which.Unresolved.Should().ContainSingle(u => u.Name == "missing" && u.Location == "parameter 'p'");
    }

    [Test]
    public void EnsureEveryVariableResolved_Should_TreatNullValueAsMissing()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT {{id}}");
        Dictionary<string, object> vars = new(StringComparer.Ordinal) { ["id"] = null! };

        // Act
        Action act = () => builder.EnsureEveryVariableResolved(vars);

        // Assert
        act.Should().Throw<UnresolvedTemplateVariableException>();
    }

    [Test]
    public void EnsureEveryVariableResolved_Should_TreatEmptyStringAsResolved()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT {{name}}");
        Dictionary<string, object> vars = new(StringComparer.Ordinal) { ["name"] = string.Empty };

        // Act
        Action act = () => builder.EnsureEveryVariableResolved(vars);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Output and return-value parameters carry no template - checking them would either be a no-op or, worse,
    /// misreport a caller's deliberately value-less parameter as an unresolved variable.
    /// </summary>
    [Test]
    public void EnsureEveryVariableResolved_Should_IgnoreOutputAndReturnValueParameters()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.StoredProcedure("MyProc")
            .WithOutputParameter("result")
            .WithOutputParameter("code", ParameterDirection.ReturnValue);
        Dictionary<string, object> vars = new(StringComparer.Ordinal);

        // Act
        Action act = () => builder.EnsureEveryVariableResolved(vars);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void EnsureEveryVariableResolved_Should_ThrowArgumentNullException_WhenVarsIsNull()
    {
        // Arrange
        SqlCommandBuilder builder = SqlCommandBuilder.Query("SELECT 1");

        // Act
        Action act = () => builder.EnsureEveryVariableResolved(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("vars");
    }
}
