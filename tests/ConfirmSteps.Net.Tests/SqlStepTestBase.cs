namespace ConfirmSteps.Net.Tests;

using System.Data.Common;

using ConfirmSteps.Steps.Sql;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

public abstract class SqlStepTestBase
{
    private SqliteConnection? anchorConnection;

    protected string ConnectionString { get; private set; } = null!;

    [SetUp]
    public void SetUp()
    {
        // A unique data source name per test avoids cross-test bleed: two tests running in shared-cache
        // mode with the same name would see each other's tables.
        string dataSource = $"testdb_{Guid.NewGuid():N}";

        ConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dataSource,
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        }.ToString();

        // SqlStep<T> opens and disposes its own connection on every execution. A plain
        // "Data Source=:memory:" database is destroyed the instant its sole connection closes, so
        // without an anchor kept open for the test's whole lifetime, a scenario with a setup connection
        // and a step connection - or two SQL steps in sequence - would find an empty database on the
        // second open. Shared cache plus one held-open anchor keeps it alive.
        anchorConnection = new SqliteConnection(ConnectionString);
        anchorConnection.Open();
    }

    [TearDown]
    public void TearDown()
    {
        anchorConnection?.Dispose();
    }

    protected void RegisterSqlite(IServiceCollection services)
    {
        services.AddExternalDbProviderFactory(SqliteFactory.Instance, ConnectionString);
    }

    protected void ExecuteSetupSql(string commandText)
    {
        using DbCommand command = anchorConnection!.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }
}
