namespace ConfirmSteps.Steps.Sql;

using System;
using System.Data.Common;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for SQL steps configuration.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers an external <see cref="DbProviderFactory"/> and connection string to be used by the SQL steps.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the provider to.</param>
    /// <param name="factory">The <see cref="DbProviderFactory"/> instance used to create database connections and commands.</param>
    /// <param name="connectionString">The connection string used to connect to the target database.</param>
    /// <returns>The <see cref="IServiceCollection"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/>, <paramref name="factory"/>, or <paramref name="connectionString"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// Registering a <see cref="DbProviderFactory"/> decouples step execution from any specific database provider,
    /// allowing SQL steps to create and manage provider-specific connections, commands, and parameters dynamically.
    /// </remarks>
    public static IServiceCollection AddExternalDbProviderFactory(this IServiceCollection services,
        DbProviderFactory factory, string connectionString)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<IDbProviderFactoryProvider>(
            new ExternalDbProviderFactoryProvider(factory, connectionString));

        return services;
    }
}
