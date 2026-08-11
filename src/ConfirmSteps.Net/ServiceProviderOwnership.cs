namespace ConfirmSteps;

/// <summary>
/// Indicates who is responsible for disposing the service provider backing a scenario.
/// </summary>
public enum ServiceProviderOwnership
{
    /// <summary>
    /// The caller created the provider and disposes it. Disposing the scenario releases nothing.
    /// </summary>
    External = 0,

    /// <summary>
    /// The scenario created the provider and disposes it along with itself.
    /// </summary>
    Scenario = 1,
}
