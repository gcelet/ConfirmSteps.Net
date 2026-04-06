namespace ConfirmSteps.Steps.Http.ResponseVerification;

using ConfirmSteps.Steps.Http.ResponseParsing;
using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// An implementation of <see cref="IHttpResponseVerifier{T}"/> that verifies the HTTP response based on its JSON content.
/// This class is designed for scenarios where the verification logic depends on the JSON structure of the response,
/// allowing for more complex assertions and validations that go beyond simple status code checks or header validations.
/// It parses the HTTP response into a <see cref="HttpResponseJson"/> object and then applies the provided
/// verification function to it, enabling detailed verification of the response content.
/// </summary>
/// <typeparam name="T">The type of the scenario data context.</typeparam>
public class HttpResponseJsonVerifier<T> : IHttpResponseVerifier<T>
  where T : class
{
  /// <summary>
  /// Initializes a new instance of the <see cref="HttpResponseJsonVerifier{T}"/> class.
  /// </summary>
  /// <param name="verifyFunc">A function that defines the verification logic to be applied to the parsed JSON response.</param>
  public HttpResponseJsonVerifier(Func<HttpResponseJson, StepContext<T>, CancellationToken, Task> verifyFunc)
  {
    VerifyFunc = verifyFunc;
  }

  private Func<HttpResponseJson, StepContext<T>, CancellationToken, Task> VerifyFunc { get; }

  /// <inheritdoc />
  public async Task Verify(StepContext<T> stepContext, HttpResponseMessage response,
    CancellationToken cancellationToken)
  {
    HttpResponseJson jsonResponse = await stepContext.ParseJson(response, cancellationToken);

    await VerifyFunc(jsonResponse, stepContext, cancellationToken);
  }
}
