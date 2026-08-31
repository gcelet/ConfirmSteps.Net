namespace ConfirmSteps.Steps.Sql.ResultParsing;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents the materialized result set returned by a SQL command execution.
/// </summary>
/// <remarks>
/// Result rows are read into memory while the database connection is open, allowing assertions and verifications
/// to run independently after the connection has been closed without keeping database resources open.
/// </remarks>
public class SqlResultSet
{
    private SqlResultSet(IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        IReadOnlyDictionary<string, object?> outputParameters)
    {
        Rows = rows;
        OutputParameters = outputParameters;
    }

    /// <summary>
    /// Gets the output, input/output, and return-value parameter values captured after the command ran.
    /// </summary>
    /// <remarks>
    /// Populated after the <see cref="DbDataReader"/> is closed, since many ADO.NET providers only make these
    /// values reliable once reading is complete. Input parameters are not included.
    /// </remarks>
    public IReadOnlyDictionary<string, object?> OutputParameters { get; }

    /// <summary>
    /// Gets the list of rows returned by the command in the order they were read.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; }

    /// <summary>
    /// Gets the total number of rows returned by the command.
    /// </summary>
    public int RowCount => Rows.Count;

    /// <summary>
    /// Asynchronously executes a <see cref="DbCommand"/> and materializes all returned rows into a <see cref="SqlResultSet"/>.
    /// </summary>
    /// <param name="command">The <see cref="DbCommand"/> to execute.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that yields the materialized <see cref="SqlResultSet"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <c>null</c>.</exception>
    public static async Task<SqlResultSet> ReadAsync(DbCommand command,
        CancellationToken cancellationToken)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        List<IReadOnlyDictionary<string, object?>> rows = new();

        using (DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken)
                   .ConfigureAwait(false))
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                Dictionary<string, object?> row = new(reader.FieldCount, StringComparer.OrdinalIgnoreCase);

                for (int field = 0; field < reader.FieldCount; field++)
                {
                    row[reader.GetName(field)] = reader.IsDBNull(field) ? null : reader.GetValue(field);
                }

                rows.Add(row);
            }
        }

        Dictionary<string, object?> outputParameters = new(StringComparer.OrdinalIgnoreCase);

        foreach (object parameterObject in command.Parameters)
        {
            DbParameter parameter = (DbParameter)parameterObject;

            if (parameter.Direction != ParameterDirection.Input)
            {
                outputParameters[parameter.ParameterName] = parameter.Value is null or DBNull ? null : parameter.Value;
            }
        }

        return new SqlResultSet(rows, outputParameters);
    }

    /// <summary>
    /// Gets the value of a specific column for a given row index.
    /// </summary>
    /// <param name="rowIndex">The zero-based index of the row.</param>
    /// <param name="columnName">The case-insensitive name of the column.</param>
    /// <returns>The column value, or <c>null</c> when the column value is null in the database.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rowIndex"/> is negative or greater than or equal to <see cref="RowCount"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no column named <paramref name="columnName"/> exists in the row.
    /// </exception>
    public object? Value(int rowIndex, string columnName)
    {
        if (rowIndex < 0 || rowIndex >= Rows.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(rowIndex), rowIndex,
                $"the command returned {Rows.Count} row(s)");
        }

        return Rows[rowIndex][columnName];
    }

    /// <summary>
    /// Gets the value of an output, input/output, or return-value parameter captured after the command ran.
    /// </summary>
    /// <param name="parameterName">The case-insensitive name of the parameter.</param>
    /// <returns>The parameter value, or <c>null</c> when the value is null.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no non-input parameter named <paramref name="parameterName"/> was declared.
    /// </exception>
    public object? OutputValue(string parameterName)
    {
        if (!OutputParameters.TryGetValue(parameterName, out object? value))
        {
            throw new KeyNotFoundException($"no output parameter named '{parameterName}'");
        }

        return value;
    }
}
