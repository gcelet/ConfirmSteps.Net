namespace ConfirmSteps.Steps.Http.Json;

using ConfirmSteps.Steps.Http.Rest;

/// <summary>
/// A verification a described step runs, supplied by the host.
/// </summary>
/// <remarks>
/// The same shape <see cref="HttpStepBuilder{T}.VerifyJson(Func{HttpResponseJson, StepContext{T}, CancellationToken, Task})"/>
/// already takes, so a verification written for a described step is a verification written for any
/// step. Failing means throwing — with whichever assertion library the host prefers.
/// </remarks>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
/// <param name="response">The response, parsed as JSON.</param>
/// <param name="stepContext">The context of the running step.</param>
/// <param name="cancellationToken">A cancellation token.</param>
/// <returns>A task that completes when the verification is done.</returns>
public delegate Task JsonStepVerification<T>(HttpResponseJson response, StepContext<T> stepContext,
    CancellationToken cancellationToken)
    where T : class;
