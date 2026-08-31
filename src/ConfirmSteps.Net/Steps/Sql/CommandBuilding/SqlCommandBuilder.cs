namespace ConfirmSteps.Steps.Sql.CommandBuilding;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using ConfirmSteps.Templating;

/// <summary>
/// Builds a parameterized SQL command from templated text and parameters.
/// </summary>
/// <remarks>
/// Command text and parameter values support templated placeholders rendered against scenario variables at execution time.
/// Parameter values are passed via provider-created <see cref="DbParameter"/> instances to prevent SQL injection and quoting issues.
/// </remarks>
public class SqlCommandBuilder
{
    private TemplateString CommandText { get; }

    private CommandType CommandType { get; }

    private Dictionary<string, SqlCommandParameter> Parameters { get; }

    private SqlCommandBuilder(TemplateString commandText, CommandType commandType)
    {
        CommandText = commandText;
        CommandType = commandType;
        Parameters = new Dictionary<string, SqlCommandParameter>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Creates a new <see cref="SqlCommandBuilder"/> for a SQL query, executed as <see cref="CommandType.Text"/>.
    /// </summary>
    /// <param name="commandText">The SQL command text, which may contain template placeholders.</param>
    /// <returns>A new <see cref="SqlCommandBuilder"/> instance for fluent configuration.</returns>
    public static SqlCommandBuilder Query(TemplateString commandText)
    {
        return new SqlCommandBuilder(commandText, CommandType.Text);
    }

    /// <summary>
    /// Creates a new <see cref="SqlCommandBuilder"/> for a stored procedure, executed as <see cref="CommandType.StoredProcedure"/>.
    /// </summary>
    /// <param name="procedureName">The stored procedure name, which may contain template placeholders.</param>
    /// <returns>A new <see cref="SqlCommandBuilder"/> instance for fluent configuration.</returns>
    public static SqlCommandBuilder StoredProcedure(TemplateString procedureName)
    {
        return new SqlCommandBuilder(procedureName, CommandType.StoredProcedure);
    }

    /// <summary>
    /// Adds a named input parameter to the SQL command.
    /// </summary>
    /// <param name="name">The name of the parameter without provider-specific prefixes.</param>
    /// <param name="value">The parameter value, which may contain template placeholders.</param>
    /// <returns>The current <see cref="SqlCommandBuilder"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
    public SqlCommandBuilder WithParameter(string name, TemplateString value)
    {
        return WithParameter(name, value, ParameterDirection.Input);
    }

    /// <summary>
    /// Adds a named input or input/output parameter to the SQL command.
    /// </summary>
    /// <param name="name">The name of the parameter without provider-specific prefixes.</param>
    /// <param name="value">The parameter's input value, which may contain template placeholders.</param>
    /// <param name="direction">
    /// The parameter direction. Must be <see cref="ParameterDirection.Input"/> or <see cref="ParameterDirection.InputOutput"/>;
    /// an <see cref="ParameterDirection.Output"/> or <see cref="ParameterDirection.ReturnValue"/> parameter has no input value
    /// to render and must be declared with <see cref="WithOutputParameter(string)"/> instead.
    /// </param>
    /// <returns>The current <see cref="SqlCommandBuilder"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="direction"/> is not <see cref="ParameterDirection.Input"/> or <see cref="ParameterDirection.InputOutput"/>.
    /// </exception>
    public SqlCommandBuilder WithParameter(string name, TemplateString value, ParameterDirection direction)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (direction is not (ParameterDirection.Input or ParameterDirection.InputOutput))
        {
            throw new ArgumentOutOfRangeException(nameof(direction), direction,
                "an Output or ReturnValue parameter has no input value; use WithOutputParameter instead");
        }

        Parameters[name] = new SqlCommandParameter(value, direction);

        return this;
    }

    /// <summary>
    /// Adds a named output parameter to the SQL command, with direction <see cref="ParameterDirection.Output"/>.
    /// </summary>
    /// <param name="name">The name of the parameter without provider-specific prefixes.</param>
    /// <returns>The current <see cref="SqlCommandBuilder"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
    public SqlCommandBuilder WithOutputParameter(string name)
    {
        return WithOutputParameter(name, ParameterDirection.Output);
    }

    /// <summary>
    /// Adds a named output or return-value parameter to the SQL command.
    /// </summary>
    /// <param name="name">The name of the parameter without provider-specific prefixes.</param>
    /// <param name="direction">
    /// The parameter direction. Must be <see cref="ParameterDirection.Output"/> or <see cref="ParameterDirection.ReturnValue"/>;
    /// an <see cref="ParameterDirection.Input"/> or <see cref="ParameterDirection.InputOutput"/> parameter needs an input value
    /// and must be declared with <see cref="WithParameter(string, TemplateString, ParameterDirection)"/> instead.
    /// </param>
    /// <returns>The current <see cref="SqlCommandBuilder"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="direction"/> is not <see cref="ParameterDirection.Output"/> or <see cref="ParameterDirection.ReturnValue"/>.
    /// </exception>
    public SqlCommandBuilder WithOutputParameter(string name, ParameterDirection direction)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (direction is not (ParameterDirection.Output or ParameterDirection.ReturnValue))
        {
            throw new ArgumentOutOfRangeException(nameof(direction), direction,
                "an Input or InputOutput parameter needs an input value; use WithParameter instead");
        }

        Parameters[name] = new SqlCommandParameter(null, direction);

        return this;
    }

    /// <summary>
    /// Validates that every variable the command text and every input parameter expect has a value, without
    /// building a <see cref="DbCommand"/>.
    /// </summary>
    /// <param name="vars">The scenario variables to check against.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="vars"/> is <c>null</c>.</exception>
    /// <exception cref="UnresolvedTemplateVariableException">
    /// Thrown when the command text or an input/input-output parameter expects a variable that has no value.
    /// </exception>
    /// <remarks>
    /// Output and return-value parameters carry no template and are not checked. A value present but <c>null</c>
    /// counts as missing, matching <see cref="TemplateString.Render"/>'s own leniency; an empty string is a value.
    /// </remarks>
    public void EnsureEveryVariableResolved(IReadOnlyDictionary<string, object> vars)
    {
        if (vars == null)
        {
            throw new ArgumentNullException(nameof(vars));
        }

        List<UnresolvedTemplateVariable> unresolved = new();

        void Check(TemplateString? template, string location)
        {
            if (template == null)
            {
                return;
            }

            foreach (string name in template.ParameterNames)
            {
                if (!vars.TryGetValue(name, out object? value) || value == null)
                {
                    unresolved.Add(new UnresolvedTemplateVariable(name, location));
                }
            }
        }

        Check(CommandText, "command text");

        foreach (KeyValuePair<string, SqlCommandParameter> parameter in Parameters)
        {
            if (parameter.Value.Direction is ParameterDirection.Input or ParameterDirection.InputOutput)
            {
                Check(parameter.Value.Value, $"parameter '{parameter.Key}'");
            }
        }

        if (unresolved.Count > 0)
        {
            throw new UnresolvedTemplateVariableException(unresolved);
        }
    }

    /// <summary>
    /// Constructs and initializes a <see cref="DbCommand"/> by rendering templated text and parameters against scenario variables.
    /// </summary>
    /// <param name="factory">The <see cref="DbProviderFactory"/> used to instantiate the command and its parameters.</param>
    /// <param name="connection">The active <see cref="DbConnection"/> to associate with the command.</param>
    /// <param name="vars">The scenario variables used to render template placeholders.</param>
    /// <returns>The constructed <see cref="DbCommand"/> ready for execution. The caller is responsible for disposing it.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/>, <paramref name="connection"/>, or <paramref name="vars"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database provider factory fails to create a command or parameter.
    /// </exception>
    /// <remarks>
    /// Instantiating commands and parameters via <see cref="DbProviderFactory"/> ensures compatibility with any ADO.NET provider.
    /// </remarks>
    public DbCommand Build(DbProviderFactory factory, DbConnection connection,
        IReadOnlyDictionary<string, object> vars)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        if (vars == null)
        {
            throw new ArgumentNullException(nameof(vars));
        }

        DbCommand command = factory.CreateCommand()
            ?? throw new InvalidOperationException("the ADO.NET factory created no command");

        try
        {
            command.Connection = connection;
            command.CommandType = CommandType;
            command.CommandText = CommandText.Render(vars);

            foreach (KeyValuePair<string, SqlCommandParameter> parameter in Parameters)
            {
                DbParameter dbParameter = factory.CreateParameter()
                    ?? throw new InvalidOperationException(
                        "the ADO.NET factory created no parameter");

                dbParameter.ParameterName = parameter.Key;
                dbParameter.Direction = parameter.Value.Direction;
                dbParameter.Value = parameter.Value.Direction is ParameterDirection.Input or ParameterDirection.InputOutput
                    ? (object)parameter.Value.Value!.Render(vars)
                    : DBNull.Value;

                command.Parameters.Add(dbParameter);
            }
        }
        catch
        {
            command.Dispose();

            throw;
        }

        return command;
    }

    private sealed record SqlCommandParameter(TemplateString? Value, ParameterDirection Direction);
}
