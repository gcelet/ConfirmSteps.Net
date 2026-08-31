namespace ConfirmSteps.Steps.Sql;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using ConfirmSteps.Steps;
using ConfirmSteps.Steps.Sql.CommandBuilding;
using ConfirmSteps.Steps.Sql.ResultParsing;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a step that executes a SQL command and verifies the returned result set.
/// </summary>
/// <typeparam name="T">The type of the custom data context.</typeparam>
/// <remarks>
/// Enables validation of database state by executing parameterized SQL queries and asserting against the materialized rows.
/// </remarks>
public class SqlStep<T> : Step<T>
    where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStep{T}"/> class.
    /// </summary>
    /// <param name="title">The title of the step.</param>
    /// <param name="commandBuilder">A factory function that constructs the <see cref="SqlCommandBuilder"/> when the step executes.</param>
    /// <param name="verifiers">A list of verification actions to validate the resulting <see cref="SqlResultSet"/>.</param>
    /// <param name="verificationMode">
    /// The mode of result set verification, determining how the verifiers are applied and how failures are handled.
    /// </param>
    /// <param name="extractors">A list of extractors to pull data from the resulting <see cref="SqlResultSet"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="commandBuilder"/>, <paramref name="verifiers"/>, or <paramref name="extractors"/> is <c>null</c>.
    /// </exception>
    public SqlStep(string title, Func<SqlCommandBuilder> commandBuilder,
        IReadOnlyList<Action<SqlResultSet, StepContext<T>>> verifiers,
        StepVerificationMode verificationMode,
        IReadOnlyList<ISqlResultSetExtractor<T>> extractors)
        : base(title, new SqlStepPreparer(commandBuilder), new SqlStepExecutor(),
            new SqlStepVerifier(verifiers, verificationMode), new SqlStepExtractor(extractors))
    {
    }

    /// <summary>
    /// Renders the SQL command's templated text and input parameters against the scenario variables and validates
    /// that none of them are missing, without opening a database connection.
    /// </summary>
    /// <remarks>
    /// A variable an HTTP step's request expects but has no value fails the step before any network call is made;
    /// the same guarantee applies here before any database connection is opened. The rendered
    /// <see cref="SqlCommandBuilder"/> is stashed on the <see cref="StepContext{T}"/> for the executor to reuse,
    /// so the factory delegate is still invoked exactly once per step execution, as before.
    /// </remarks>
    private sealed class SqlStepPreparer : IStepPreparer<T>
    {
        public SqlStepPreparer(Func<SqlCommandBuilder> commandBuilder)
        {
            if (commandBuilder == null)
            {
                throw new ArgumentNullException(nameof(commandBuilder));
            }

            CommandBuilderFactory = commandBuilder;
        }

        private Func<SqlCommandBuilder> CommandBuilderFactory { get; }

        public Task<ConfirmStatus> PrepareStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            SqlCommandBuilder commandBuilder = CommandBuilderFactory();

            commandBuilder.EnsureEveryVariableResolved(stepContext.Vars);

            stepContext.AddItem(commandBuilder);

            return Task.FromResult(ConfirmStatus.Success);
        }
    }

    private sealed class SqlStepExecutor : IStepExecutor<T>
    {
        public async Task<ConfirmStatus> ExecuteStep(StepContext<T> stepContext,
            CancellationToken cancellationToken)
        {
            if (stepContext == null)
            {
                throw new ArgumentNullException(nameof(stepContext));
            }

            if (!stepContext.TryGetItem(out SqlCommandBuilder? commandBuilder) || commandBuilder == null)
            {
                return ConfirmStatus.Failure;
            }

            IDbProviderFactoryProvider factoryProvider =
                stepContext.Services.GetRequiredService<IDbProviderFactoryProvider>();
            DbProviderFactory factory = factoryProvider.Provide();
            DbConnection connection = factory.CreateConnection()
                ?? throw new InvalidOperationException("the ADO.NET factory created no connection");

            await using (connection.ConfigureAwait(false))
            {
                connection.ConnectionString = factoryProvider.ProvideConnectionString();

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                SqlResultSet resultSet;

                using (DbCommand command = commandBuilder.Build(factory, connection, stepContext.Vars))
                {
                    resultSet = await SqlResultSet.ReadAsync(command, cancellationToken)
                        .ConfigureAwait(false);
                }

                stepContext.AddItem(resultSet);
            }

            return ConfirmStatus.Success;
        }
    }

    /// <summary>
    /// Verifies the materialized <see cref="SqlResultSet"/> against the configured verifiers.
    /// </summary>
    /// <remarks>
    /// Mirrors <c>HttpStepVerifier</c>: under <see cref="StepVerificationMode.StopOnFirstFailure"/> (the default),
    /// the first failing verifier stops the chain and its exception surfaces as-is; under
    /// <see cref="StepVerificationMode.VerifyAll"/>, every verifier runs regardless of earlier failures and their
    /// exceptions are combined into an <see cref="AggregateException"/> when there is more than one.
    /// </remarks>
    private sealed class SqlStepVerifier : IStepVerifier<T>
    {
        public SqlStepVerifier(IReadOnlyList<Action<SqlResultSet, StepContext<T>>> verifiers,
            StepVerificationMode verificationMode)
        {
            if (verifiers == null)
            {
                throw new ArgumentNullException(nameof(verifiers));
            }

            Verifiers = verifiers;
            VerificationMode = verificationMode;
        }

        private StepVerificationMode VerificationMode { get; }

        private IReadOnlyList<Action<SqlResultSet, StepContext<T>>> Verifiers { get; }

        public Task<ConfirmStatus> VerifyStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            if (!stepContext.TryGetItem(out SqlResultSet? resultSet) || resultSet == null)
            {
                return Task.FromResult(ConfirmStatus.Failure);
            }

            List<Exception> exceptions = new();

            foreach (Action<SqlResultSet, StepContext<T>> verifier in Verifiers)
            {
                try
                {
                    verifier(resultSet, stepContext);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);

                    if (VerificationMode == StepVerificationMode.StopOnFirstFailure)
                    {
                        break;
                    }
                }
            }

            return Task.FromResult(exceptions.Count switch
            {
                1 => throw exceptions[0],
                > 1 => throw new AggregateException(exceptions),
                _ => ConfirmStatus.Success
            });
        }
    }

    private sealed class SqlStepExtractor : IStepExtractor<T>
    {
        public SqlStepExtractor(IReadOnlyList<ISqlResultSetExtractor<T>> extractors)
        {
            if (extractors == null)
            {
                throw new ArgumentNullException(nameof(extractors));
            }

            Extractors = extractors;
        }

        private IReadOnlyList<ISqlResultSetExtractor<T>> Extractors { get; }

        public async Task<ConfirmStatus> ExtractStep(StepContext<T> stepContext, CancellationToken cancellationToken)
        {
            if (Extractors.Count == 0)
            {
                return ConfirmStatus.Success;
            }

            if (!stepContext.TryGetItem(out SqlResultSet? resultSet) || resultSet == null)
            {
                return ConfirmStatus.Failure;
            }

            foreach (ISqlResultSetExtractor<T> extractor in Extractors)
            {
                await extractor.Extract(stepContext, resultSet, cancellationToken).ConfigureAwait(false);
            }

            return ConfirmStatus.Success;
        }
    }
}
