namespace ConfirmSteps.Steps.Http.ResponseVerification;

using ConfirmSteps.Steps.Http.ResponseParsing;
using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// An implementation of <see cref="IHttpResponseVerifier{T}"/> that verifies the HTTP response based on its REST API result.
/// This class is intended for scenarios where the verification logic is centered around the REST API result derived
/// from the HTTP response, allowing for assertions and validations that are specific to RESTful API responses.
/// It parses the HTTP response into a <see cref="RestApiResult"/> object and then applies the provided
/// verification function to it, enabling detailed verification of the REST API response content and structure.
/// </summary>
/// <typeparam name="T">The type of the scenario data context.</typeparam>
public class RestApiResultVerifier<T> : IHttpResponseVerifier<T>
  where T : class
{
  /// <summary>
  /// Initializes a new instance of the <see cref="RestApiResultVerifier{T}"/> class.
  /// </summary>
  /// <param name="verifyFunc">A function that defines the verification logic to be applied to the parsed REST API result.</param>
  public RestApiResultVerifier(Func<RestApiResult, StepContext<T>, CancellationToken, Task> verifyFunc)
  {
    VerifyFunc = verifyFunc;
  }

  private Func<RestApiResult, StepContext<T>, CancellationToken, Task> VerifyFunc { get; }

  /// <inheritdoc />
  public async Task Verify(StepContext<T> stepContext, HttpResponseMessage response,
    CancellationToken cancellationToken)
  {
    RestApiResult restApiResult = await stepContext.ParseRestApi(response, cancellationToken);

    await VerifyFunc(restApiResult, stepContext, cancellationToken);
  }
}

/// <summary>
/// An implementation of <see cref="IHttpResponseVerifier{T}"/> that verifies the HTTP response based on its typed REST API result.
/// This class is designed for scenarios where the verification logic is focused on a specific type of REST API result derived
/// from the HTTP response, allowing for more targeted assertions and validations that are specific
/// to the expected structure and content of the REST API response.
/// It parses the HTTP response into a <see cref="RestApiResult{TResult}"/> object and then applies the provided verification function to it,
/// </summary>
/// <typeparam name="T">The type of the scenario data context.</typeparam>
/// <typeparam name="TResult">The type of the expected result data from the REST API response.</typeparam>
public class RestApiResultVerifier<T, TResult> : IHttpResponseVerifier<T>
  where T : class
  where TResult : class
{
  /// <summary>
  /// Initializes a new instance of the <see cref="RestApiResultVerifier{T, TResult}"/> class.
  /// </summary>
  /// <param name="verifyFunc">
  /// A function that defines the verification logic to be applied to the parsed typed REST API result.
  /// </param>
  public RestApiResultVerifier(Func<RestApiResult<TResult>, StepContext<T>, CancellationToken, Task> verifyFunc)
  {
    VerifyFunc = verifyFunc;
  }

  private Func<RestApiResult<TResult>, StepContext<T>, CancellationToken, Task> VerifyFunc { get; }

  /// <inheritdoc />
  public async Task Verify(StepContext<T> stepContext, HttpResponseMessage response,
    CancellationToken cancellationToken)
  {
    RestApiResult<TResult> restApiResult = await stepContext.ParseRestApi<T, TResult>(response, cancellationToken);

    await VerifyFunc(restApiResult, stepContext, cancellationToken);
  }
}
