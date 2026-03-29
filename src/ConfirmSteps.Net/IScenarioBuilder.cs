namespace ConfirmSteps;

/// <summary>
/// Provides a builder for creating a <see cref="Scenario{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IScenarioBuilder<T>
    where T : class
{
    /// <summary>
    /// Builds and returns the configured scenario.
    /// </summary>
    /// <returns>The constructed <see cref="Scenario{T}"/> instance.</returns>
    Scenario<T> Build();
}
