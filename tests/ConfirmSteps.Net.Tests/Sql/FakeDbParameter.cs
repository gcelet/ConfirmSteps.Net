namespace ConfirmSteps.Net.Tests.Sql;

using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A bare <see cref="DbParameter"/> that stores whatever is assigned without validation.
/// </summary>
/// <remarks>
/// Microsoft.Data.Sqlite's own parameter type rejects any <see cref="ParameterDirection"/> other than
/// <see cref="ParameterDirection.Input"/> the instant it is set, and its command type rejects
/// <see cref="CommandType.StoredProcedure"/> the instant it is set - so neither can be used to observe what
/// <see cref="ConfirmSteps.Steps.Sql.CommandBuilding.SqlCommandBuilder"/> positions on a <see cref="DbCommand"/>/
/// <see cref="DbParameter"/> for those cases. This fake exists only where a real ADO.NET provider genuinely
/// cannot represent the state under test.
/// </remarks>
internal sealed class FakeDbParameter : DbParameter
{
    private string parameterName = string.Empty;

    private string sourceColumn = string.Empty;

    public override DbType DbType { get; set; }

    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;

    public override bool IsNullable { get; set; }

    [AllowNull]
    public override string ParameterName
    {
        get => parameterName;
        set => parameterName = value ?? string.Empty;
    }

    public override int Size { get; set; }

    [AllowNull]
    public override string SourceColumn
    {
        get => sourceColumn;
        set => sourceColumn = value ?? string.Empty;
    }

    public override bool SourceColumnNullMapping { get; set; }

    public override DataRowVersion SourceVersion { get; set; }

    public override object? Value { get; set; }

    public override void ResetDbType()
    {
        DbType = default;
    }
}
