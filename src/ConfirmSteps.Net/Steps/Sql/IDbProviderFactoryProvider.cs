namespace ConfirmSteps.Steps.Sql;

using System.Data.Common;

/// <summary>
/// Defines a provider for an ADO.NET <see cref="DbProviderFactory"/> and its associated connection string.
/// </summary>
/// <remarks>
/// A <see cref="DbProviderFactory"/> provides database provider neutrality across ADO.NET implementations by creating
/// connections, commands, and parameters uniformly, without coupling SQL steps to a specific database provider.
/// </remarks>
public interface IDbProviderFactoryProvider
{
    /// <summary>
    /// Provides the <see cref="DbProviderFactory"/> instance.
    /// </summary>
    /// <returns>The configured <see cref="DbProviderFactory"/> instance.</returns>
    DbProviderFactory Provide();

    /// <summary>
    /// Provides the connection string used to connect to the database.
    /// </summary>
    /// <returns>The connection string.</returns>
    string ProvideConnectionString();
}
