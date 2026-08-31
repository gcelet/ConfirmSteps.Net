namespace ConfirmSteps.Net.Tests.Sql;

using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A <see cref="DbCommand"/> that stores whatever is assigned without validation and returns an empty
/// <see cref="FakeDbDataReader"/> when executed, paired with <see cref="FakeDbParameter"/>/
/// <see cref="FakeDbParameterCollection"/>.
/// </summary>
/// <remarks>
/// See <see cref="FakeDbParameter"/> for why: Microsoft.Data.Sqlite's own command/parameter types reject
/// <see cref="CommandType.StoredProcedure"/> and any non-<see cref="ParameterDirection.Input"/> direction the
/// instant either is set, so they cannot observe what
/// <see cref="ConfirmSteps.Steps.Sql.CommandBuilding.SqlCommandBuilder"/> positions for those cases.
/// </remarks>
internal sealed class FakeDbCommand : DbCommand
{
    private readonly FakeDbParameterCollection parameters = new();

    private string commandText = string.Empty;

    [AllowNull]
    public override string CommandText
    {
        get => commandText;
        set => commandText = value ?? string.Empty;
    }

    public override int CommandTimeout { get; set; }

    public override CommandType CommandType { get; set; } = CommandType.Text;

    public override bool DesignTimeVisible { get; set; }

    public override UpdateRowSource UpdatedRowSource { get; set; }

    protected override DbConnection? DbConnection { get; set; }

    protected override DbParameterCollection DbParameterCollection => parameters;

    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel()
    {
    }

    protected override DbParameter CreateDbParameter()
    {
        return new FakeDbParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return new FakeDbDataReader();
    }

    public override int ExecuteNonQuery()
    {
        throw new NotSupportedException();
    }

    public override object? ExecuteScalar()
    {
        throw new NotSupportedException();
    }

    public override void Prepare()
    {
    }
}
