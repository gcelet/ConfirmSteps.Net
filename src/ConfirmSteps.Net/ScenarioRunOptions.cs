namespace ConfirmSteps;

/// <summary>
/// Host choices that apply to one execution of a scenario rather than to how it was built.
/// </summary>
/// <remarks>
/// <para>
/// Run-scoped rather than scenario-scoped because a single built scenario is meant to be reused: the
/// same compiled journey can serve a functional test that inspects its responses and a measurement
/// run that only wants status and timings, and those two want different answers here.
/// </para>
/// <para>
/// What does <b>not</b> belong here: anything that changes what the outcome of a scenario means. A
/// step that does not succeed fails the scenario, and that is not a setting. Where a host may
/// legitimately influence execution, the decision is delegated through an extension point instead —
/// see <see cref="IStepFailurePolicy{T}"/>.
/// </para>
/// </remarks>
public sealed record ScenarioRunOptions
{
    /// <summary>
    /// Gets the options a scenario runs under when the caller does not supply any.
    /// </summary>
    /// <remarks>
    /// Every default reproduces the behaviour scenarios have always had, so the overload taking no
    /// options and this one are interchangeable for existing code.
    /// </remarks>
    public static ScenarioRunOptions Default { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the items a step parked in its context are disposed once the
    /// step is over.
    /// </summary>
    /// <remarks>
    /// <para>
    /// HTTP steps park the request, the buffered response and its parsed form in the step context,
    /// and nothing releases them: the buffered body of every response stays alive for as long as the
    /// <see cref="ConfirmStepResult{T}"/> does. Over a long run with large payloads that adds up to
    /// the whole traffic of the run held in memory.
    /// </para>
    /// <para>
    /// Off by default because it is a behavioural change: a caller may legitimately read a response
    /// from <c>StepContext.Items</c> after the step. Turn it on when results are consumed for their
    /// status and timings rather than their content.
    /// </para>
    /// </remarks>
    public bool DisposeStepItems { get; init; }
}
