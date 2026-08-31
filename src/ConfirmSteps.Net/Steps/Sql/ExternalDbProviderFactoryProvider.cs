namespace ConfirmSteps.Steps.Sql;

using System;
using System.Data.Common;

/// <summary>
/// Provides an implementation of <see cref="IDbProviderFactoryProvider"/> that uses an externally supplied <see cref="DbProviderFactory"/> and connection string.
/// </summary>
public class ExternalDbProviderFactoryProvider : IDbProviderFactoryProvider
{
    private DbProviderFactory Factory { get; }

    private string ConnectionString { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalDbProviderFactoryProvider"/> class with the specified database provider factory and connection string.
    /// </summary>
    /// <param name="factory">The <see cref="DbProviderFactory"/> instance used to create database connections and commands.</param>
    /// <param name="connectionString">The connection string used to open database connections.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> or <paramref name="connectionString"/> is <c>null</c>.
    /// </exception>
    public ExternalDbProviderFactoryProvider(DbProviderFactory factory, string connectionString)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        Factory = factory;
        ConnectionString = connectionString;
    }

    /// <inheritdoc />
    public DbProviderFactory Provide()
    {
        return Factory;
    }

    /// <inheritdoc />
    public string ProvideConnectionString()
    {
        return ConnectionString;
    }
}
