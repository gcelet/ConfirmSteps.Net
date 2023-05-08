namespace ConfirmSteps.Steps.Http;

using System.Net;

public interface ICookieContainerProvider
{
    CookieContainer Provide();
}