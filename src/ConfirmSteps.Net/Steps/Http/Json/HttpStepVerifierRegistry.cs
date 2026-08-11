namespace ConfirmSteps.Steps.Http.Json;

using System.Text.Json.Nodes;

/// <summary>
/// The verifications a host makes available to described steps, by name.
/// </summary>
/// <remarks>
/// <para>
/// This library ships no assertions and is not about to start. What a response ought to contain — a
/// status worth accepting, a list worth expecting, a total worth checking — is a judgement about the
/// system under test, and it belongs to whoever is testing it, written with whichever assertion
/// library they already use.
/// </para>
/// <para>
/// So a description does not express assertions: it <b>names</b> them. A <c>verify</c> entry carries a
/// <c>kind</c> that this registry resolves, and the rest of the entry is handed to the host's factory
/// as JSON the library never interprets. Naming a kind nothing registered is refused when the step is
/// built, not discovered when it runs.
/// </para>
/// <para>
/// Even the HTTP status is the host's business. A step may legitimately expect a 500, in which case a
/// 200 is the failure — deciding otherwise here would impose a view of success that is not this
/// library's to hold.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the data object the scenario operates on.</typeparam>
public sealed class HttpStepVerifierRegistry<T>
    where T : class
{
    private readonly Dictionary<string, Func<JsonNode, HttpStepVerification<T>>> raw =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, Func<JsonNode, JsonStepVerification<T>>> json =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the kinds registered so far, for a message that has to list them.</summary>
    public IReadOnlyCollection<string> Kinds => [.. raw.Keys, .. json.Keys];

    /// <summary>
    /// Makes a kind of verification available to descriptions, reading the response unparsed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The factory receives the whole <c>verify</c> entry, so a kind reads its own settings out of it —
    /// <c>{ "kind": "status", "expect": [200, 206] }</c> is entirely the host's to interpret.
    /// Registering the same kind twice replaces it, which is what makes a default overridable.
    /// </para>
    /// <para>
    /// This is the shape for a check that does not read the body. It costs no parsing, and it works on
    /// a response that has no body — which a status check must.
    /// </para>
    /// </remarks>
    /// <param name="kind">Name a description uses to ask for this verification.</param>
    /// <param name="factory">Builds the verification from the description entry.</param>
    /// <returns>The registry, for fluent chaining.</returns>
    public HttpStepVerifierRegistry<T> Register(string kind,
        Func<JsonNode, HttpStepVerification<T>> factory)
    {
        raw[kind] = factory;
        json.Remove(kind);

        return this;
    }

    /// <summary>
    /// Makes a kind of verification available to descriptions, reading the response as JSON.
    /// </summary>
    /// <remarks>
    /// For a check that reads the body. Declaring one is what makes a step pay for parsing its
    /// response, which is worth knowing when the step is played thousands of times.
    /// </remarks>
    /// <param name="kind">Name a description uses to ask for this verification.</param>
    /// <param name="factory">Builds the verification from the description entry.</param>
    /// <returns>The registry, for fluent chaining.</returns>
    public HttpStepVerifierRegistry<T> RegisterJson(string kind,
        Func<JsonNode, JsonStepVerification<T>> factory)
    {
        json[kind] = factory;
        raw.Remove(kind);

        return this;
    }

    /// <summary>
    /// Adds the verification a description entry asks for to a step being built.
    /// </summary>
    /// <param name="kind">Name the description used.</param>
    /// <param name="entry">The whole entry, handed to the factory uninterpreted.</param>
    /// <param name="builder">The step being built.</param>
    /// <exception cref="HttpStepDescriptionException">
    /// When nothing is registered under that name.
    /// </exception>
    public void ApplyTo(string kind, JsonNode entry, HttpStepBuilder<T> builder)
    {
        if (raw.TryGetValue(kind, out Func<JsonNode, HttpStepVerification<T>>? rawFactory))
        {
            HttpStepVerification<T> verify = rawFactory(entry);

            builder.Verify((response, context, cancellationToken) =>
                verify(response, context, cancellationToken));

            return;
        }

        if (json.TryGetValue(kind, out Func<JsonNode, JsonStepVerification<T>>? jsonFactory))
        {
            JsonStepVerification<T> verify = jsonFactory(entry);

            builder.VerifyJson((response, context, cancellationToken) =>
                verify(response, context, cancellationToken));

            return;
        }

        throw HttpStepDescriptionException.UnknownVerificationKind(kind, Kinds);
    }
}
