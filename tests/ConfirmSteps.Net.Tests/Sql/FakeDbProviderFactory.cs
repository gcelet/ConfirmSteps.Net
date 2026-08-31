namespace ConfirmSteps.Net.Tests.Sql;

using System.Data.Common;

/// <summary>
/// A <see cref="DbProviderFactory"/> that creates <see cref="FakeDbCommand"/>/<see cref="FakeDbParameter"/>
/// instances, for the parts of <see cref="ConfirmSteps.Steps.Sql.CommandBuilding.SqlCommandBuilder"/>'s contract
/// that a real ADO.NET provider like SQLite cannot represent (see <see cref="FakeDbParameter"/>).
/// </summary>
internal sealed class FakeDbProviderFactory : DbProviderFactory
{
    public static readonly FakeDbProviderFactory Instance = new();

    public override DbCommand CreateCommand()
    {
        return new FakeDbCommand();
    }

    public override DbParameter CreateParameter()
    {
        return new FakeDbParameter();
    }
}
