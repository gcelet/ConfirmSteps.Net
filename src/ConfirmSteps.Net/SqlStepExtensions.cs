namespace ConfirmSteps;

using System;

using ConfirmSteps.Steps.Sql;
using ConfirmSteps.Steps.Sql.CommandBuilding;

/// <summary>
/// Provides extension methods for adding SQL-based steps to a scenario.
/// </summary>
public static class SqlStepExtensions
{
    /// <summary>
    /// Adds a SQL-based step to the scenario.
    /// </summary>
    /// <remarks>
    /// Database connections and commands are created using the <see cref="IDbProviderFactoryProvider"/> registered
    /// in the scenario services, typically configured via <see cref="Extensions.AddExternalDbProviderFactory"/>.
    /// </remarks>
    /// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
    /// <param name="stepBuilderAppender">The step builder appender.</param>
    /// <param name="title">The title of the step.</param>
    /// <param name="commandBuilder">A function that returns a <see cref="SqlCommandBuilder"/> configured for the SQL command.</param>
    /// <param name="stepBuilder">An action to configure the SQL step verifications.</param>
    /// <returns>The <see cref="IStepBuilderAppender{T}"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stepBuilderAppender"/> or <paramref name="stepBuilder"/> is <c>null</c>.
    /// </exception>
    public static IStepBuilderAppender<T> SqlStep<T>(
        this IStepBuilderAppender<T> stepBuilderAppender,
        string title, Func<SqlCommandBuilder> commandBuilder, Action<SqlStepBuilder<T>> stepBuilder)
        where T : class
    {
        if (stepBuilderAppender == null)
        {
            throw new ArgumentNullException(nameof(stepBuilderAppender));
        }

        if (stepBuilder == null)
        {
            throw new ArgumentNullException(nameof(stepBuilder));
        }

        SqlStepBuilder<T> builder = new(title, commandBuilder);

        stepBuilder(builder);

        stepBuilderAppender.Append(builder);

        return stepBuilderAppender;
    }
}
