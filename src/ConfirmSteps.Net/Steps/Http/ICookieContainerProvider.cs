namespace ConfirmSteps.Steps.Http;

using System.Net;

/// <summary>
/// Defines a provider for a <see cref="CookieContainer"/> instance.
/// </summary>
public interface ICookieContainerProvider
{
    /// <summary>
    /// Provides a <see cref="CookieContainer"/> instance.
    /// </summary>
    /// <returns>The <see cref="CookieContainer"/> instance.</returns>
    CookieContainer Provide();
}
