namespace ConfirmSteps.Net.Tests.Sql.ResultParsing;

using System.Data;
using System.Data.Common;

using AwesomeAssertions;

using ConfirmSteps.Steps.Sql.ResultParsing;

using Microsoft.Data.Sqlite;

[TestFixture]
public class SqlResultSetTests
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
    public async Task ReadAsync_Should_MaterializeAllRowsInOrder()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText =
            "SELECT 1 AS Id, 'a' AS Name UNION ALL SELECT 2, 'b' UNION ALL SELECT 3, 'c'";

        // Act
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Assert
        resultSet.RowCount.Should().Be(3);
        resultSet.Value(0, "Id").Should().Be(1L);
        resultSet.Value(0, "Name").Should().Be("a");
        resultSet.Value(1, "Id").Should().Be(2L);
        resultSet.Value(1, "Name").Should().Be("b");
        resultSet.Value(2, "Id").Should().Be(3L);
        resultSet.Value(2, "Name").Should().Be("c");
    }

    [Test]
    public async Task ReadAsync_Should_ConvertDbNullToClrNull()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT NULL AS Note";

        // Act
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Assert
        resultSet.Value(0, "Note").Should().BeNull();
    }

    [Test]
    public async Task ReadAsync_Should_MaterializeEmptyResultSet_WhenQueryMatchesNoRows()
    {
        // Arrange
        using (DbCommand createTable = connection.CreateCommand())
        {
            createTable.CommandText = "CREATE TABLE T (Id INTEGER)";
            await createTable.ExecuteNonQueryAsync();
        }

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM T";

        // Act
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Assert
        resultSet.RowCount.Should().Be(0);
        resultSet.Rows.Should().BeEmpty();
    }

    [Test]
    public async Task ReadAsync_Should_ThrowArgumentNullException_WhenCommandIsNull()
    {
        // Act
        Func<Task> act = () => SqlResultSet.ReadAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("command");
    }

    [Test]
    public async Task Value_Should_BeCaseInsensitiveOnColumnName()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1 AS Name";
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Act & Assert
        resultSet.Value(0, "Name").Should().Be(resultSet.Value(0, "name"));
        resultSet.Value(0, "Name").Should().Be(resultSet.Value(0, "NAME"));
    }

    [Test]
    public async Task Value_Should_ThrowArgumentOutOfRangeException_WhenRowIndexIsNegative()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1 AS Id";
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Act
        Action act = () => resultSet.Value(-1, "Id");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("rowIndex")
            .WithMessage("*the command returned 1 row(s)*");
    }

    [Test]
    public async Task Value_Should_ThrowArgumentOutOfRangeException_WhenRowIndexIsPastTheLastRow()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1 AS Id";
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Act
        Action act = () => resultSet.Value(1, "Id");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("rowIndex")
            .WithMessage("*the command returned 1 row(s)*");
    }

    [Test]
    public async Task Value_Should_ThrowKeyNotFoundException_WhenColumnDoesNotExist()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1 AS Id";
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Act
        Action act = () => resultSet.Value(0, "DoesNotExist");

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }

    /// <summary>
    /// A query returning two columns with the same name (e.g. an unaliased join) silently loses the first
    /// value to the second, since rows are stored as a plain dictionary keyed by column name. This pins down
    /// the footgun rather than leaving it to be discovered against a real database.
    /// </summary>
    [Test]
    public async Task ReadAsync_Should_KeepLastValue_WhenQueryReturnsDuplicateColumnNames()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1 AS Id, 2 AS Id";

        // Act
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Assert
        resultSet.Value(0, "Id").Should().Be(2L);
    }

    /// <summary>
    /// Microsoft.Data.Sqlite's own parameter type refuses <see cref="ParameterDirection.Output"/> the instant it
    /// is set, so a <see cref="FakeDbCommand"/>/<see cref="FakeDbParameter"/> stand in to pre-seed the value/
    /// direction manually. This pins down the capture plumbing (walk <c>command.Parameters</c> after the reader
    /// closes, keep non-Input directions) - not a claim that SQLite genuinely supports output parameters.
    /// </summary>
    [Test]
    public async Task ReadAsync_Should_ExposeNonInputParameterValues_AfterReaderCloses()
    {
        // Arrange
        using FakeDbCommand command = new() { CommandText = "SELECT 1" };
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = "result";
        parameter.Direction = ParameterDirection.Output;
        parameter.Value = "seeded";
        command.Parameters.Add(parameter);

        // Act
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Assert
        resultSet.OutputValue("result").Should().Be("seeded");
    }

    [Test]
    public async Task ReadAsync_Should_NotExposeInputParameters_InOutputParameters()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = "input";
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = "value";
        command.Parameters.Add(parameter);

        // Act
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Assert
        resultSet.OutputParameters.Should().NotContainKey("input");
    }

    [Test]
    public async Task ReadAsync_Should_ConvertDbNullOutputParameterValueToClrNull()
    {
        // Arrange
        using FakeDbCommand command = new() { CommandText = "SELECT 1" };
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = "result";
        parameter.Direction = ParameterDirection.Output;
        parameter.Value = DBNull.Value;
        command.Parameters.Add(parameter);

        // Act
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Assert
        resultSet.OutputValue("result").Should().BeNull();
    }

    [Test]
    public async Task OutputValue_Should_BeCaseInsensitiveOnParameterName()
    {
        // Arrange
        using FakeDbCommand command = new() { CommandText = "SELECT 1" };
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = "Result";
        parameter.Direction = ParameterDirection.Output;
        parameter.Value = "seeded";
        command.Parameters.Add(parameter);
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Act & Assert
        resultSet.OutputValue("result").Should().Be("seeded");
    }

    [Test]
    public async Task OutputValue_Should_ThrowKeyNotFoundException_WhenParameterNameDoesNotExist()
    {
        // Arrange
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        SqlResultSet resultSet = await SqlResultSet.ReadAsync(command, CancellationToken.None);

        // Act
        Action act = () => resultSet.OutputValue("doesNotExist");

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }
}
