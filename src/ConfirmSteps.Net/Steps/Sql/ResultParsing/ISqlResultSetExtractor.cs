namespace ConfirmSteps.Steps.Sql.ResultParsing;

/// <summary>
/// Extracts data from a <see cref="SqlResultSet"/>.
/// </summary>
/// <typeparam name="T">The type of the scenario data.</typeparam>
public interface ISqlResultSetExtractor<T>
    where T : class
{
    /// <summary>
    /// Extracts data from the given <see cref="SqlResultSet"/>.
    /// </summary>
    /// <param name="stepContext">The context of the step being executed.</param>
    /// <param name="resultSet">The result set to extract data from.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    Task Extract(StepContext<T> stepContext, SqlResultSet resultSet, CancellationToken cancellationToken);
}
