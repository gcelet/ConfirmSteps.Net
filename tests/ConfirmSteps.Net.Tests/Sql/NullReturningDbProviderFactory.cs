namespace ConfirmSteps.Net.Tests.Sql;

using System.Data.Common;

using Microsoft.Data.Sqlite;

/// <summary>
/// Wraps the real <see cref="SqliteFactory"/> but lets one creation method be forced to return null,
/// to exercise the "the ADO.NET factory created no X" guards that a contract-compliant provider like
/// SQLite can never trigger on its own.
/// </summary>
internal sealed class NullReturningDbProviderFactory : DbProviderFactory
{
    public bool ReturnNullConnection { get; init; }

    public bool ReturnNullCommand { get; init; }

    public bool ReturnNullParameter { get; init; }

    public override DbConnection? CreateConnection()
    {
        return ReturnNullConnection ? null : SqliteFactory.Instance.CreateConnection();
    }

    public override DbCommand? CreateCommand()
    {
        return ReturnNullCommand ? null : SqliteFactory.Instance.CreateCommand();
    }

    public override DbParameter? CreateParameter()
    {
        return ReturnNullParameter ? null : SqliteFactory.Instance.CreateParameter();
    }
}
