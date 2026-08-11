namespace ConfirmSteps.Steps.Http.Json;

/// <summary>
/// A verification a described step runs on the raw response, supplied by the host.
/// </summary>
/// <remarks>
/// The kind to register whenever the check does not read the body — a status, a header, a content
/// length. It costs no parsing, which matters under load: a status check that parsed every response
/// would measure the generator as much as the target, and would fail outright on a response with no
/// body at all.
/// <para>
/// Reading the body is the other shape, <see cref="JsonStepVerification{T}"/>. A host may also parse
/// on its own terms from here, through <c>stepContext.ParseJson</c>.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
/// <param name="response">The response, unparsed.</param>
/// <param name="stepContext">The context of the running step.</param>
/// <param name="cancellationToken">A cancellation token.</param>
/// <returns>A task that completes when the verification is done.</returns>
public delegate Task HttpStepVerification<T>(HttpResponseMessage response, StepContext<T> stepContext,
    CancellationToken cancellationToken)
    where T : class;
