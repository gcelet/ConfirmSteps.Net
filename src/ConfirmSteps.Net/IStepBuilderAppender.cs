namespace ConfirmSteps;

using ConfirmSteps.Internal;

/// <summary>
/// Provides a mechanism to append step builders to a scenario.
/// </summary>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public interface IStepBuilderAppender<T>
    where T : class
{
    /// <summary>
    /// Appends a step builder to the scenario step list.
    /// </summary>
    /// <param name="stepBuilder">The step builder to append.</param>
    void Append(IStepBuilder<T> stepBuilder);
}
