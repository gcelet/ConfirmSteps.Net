namespace ConfirmSteps.Steps.Sql;

using System;
using System.Collections.Generic;

using ConfirmSteps.Internal;
using ConfirmSteps.Steps;
using ConfirmSteps.Steps.Sql.CommandBuilding;
using ConfirmSteps.Steps.Sql.ResultParsing;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides a builder for creating a SQL-based step.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public class SqlStepBuilder<T> : IStepBuilder<T>
    where T : class
{
    private string Title { get; }

    private Func<SqlCommandBuilder> CommandBuilder { get; }

    private List<ISqlResultSetExtractor<T>> Extractors { get; } = new();

    private StepVerificationMode VerificationMode { get; set; } = StepVerificationMode.StopOnFirstFailure;

    private List<Action<SqlResultSet, StepContext<T>>> Verifiers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStepBuilder{T}"/> class.
    /// </summary>
    /// <param name="title">The title of the step.</param>
    /// <param name="commandBuilder">A function that returns a <see cref="SqlCommandBuilder"/> configured for the SQL command.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="title"/> or <paramref name="commandBuilder"/> is <c>null</c>.
    /// </exception>
    public SqlStepBuilder(string title, Func<SqlCommandBuilder> commandBuilder)
    {
        if (title == null)
        {
            throw new ArgumentNullException(nameof(title));
        }

        if (commandBuilder == null)
        {
            throw new ArgumentNullException(nameof(commandBuilder));
        }

        Title = title;
        CommandBuilder = commandBuilder;
        Verifiers = new List<Action<SqlResultSet, StepContext<T>>>();
    }

    /// <summary>
    /// Configures verification logic for the result set returned by the SQL command.
    /// </summary>
    /// <param name="verify">The verification action to validate the returned <see cref="SqlResultSet"/>.</param>
    /// <returns>The current <see cref="SqlStepBuilder{T}"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="verify"/> is <c>null</c>.</exception>
    public SqlStepBuilder<T> VerifyRows(Action<SqlResultSet, StepContext<T>> verify)
    {
        if (verify == null)
        {
            throw new ArgumentNullException(nameof(verify));
        }

        Verifiers.Add(verify);

        return this;
    }

    /// <summary>
    /// Configures data extraction from the result set returned by the SQL command.
    /// </summary>
    /// <param name="extract">An action to configure the extraction using <see cref="SqlResultSetExtractionBuilder{T}"/>.</param>
    /// <returns>The current <see cref="SqlStepBuilder{T}"/> for fluent chaining.</returns>
    public SqlStepBuilder<T> Extract(Action<SqlResultSetExtractionBuilder<T>> extract)
    {
        SqlResultSetExtractionBuilder<T> extractionBuilder = new();

        extract(extractionBuilder);

        Extractors.AddRange(((ISqlResultSetExtractorProvider<T>)extractionBuilder).Provide());

        return this;
    }

    /// <summary>
    /// Configures the mode of result set verification, determining how the verifiers are applied to the result set
    /// and how failures are handled during the verification process.
    /// </summary>
    /// <param name="verificationMode">The mode of result set verification to be applied for this step.</param>
    /// <returns>The current <see cref="SqlStepBuilder{T}"/> for fluent chaining.</returns>
    public SqlStepBuilder<T> WithVerificationMode(StepVerificationMode verificationMode)
    {
        VerificationMode = verificationMode;

        return this;
    }

    /// <inheritdoc />
    IStep<T> IStepBuilder<T>.Build()
    {
        return new SqlStep<T>(Title, CommandBuilder, Verifiers, VerificationMode, Extractors);
    }

    /// <inheritdoc />
    IServiceCollection IStepBuilder<T>.RegisterServices(IServiceCollection services)
    {
        return services;
    }
}
