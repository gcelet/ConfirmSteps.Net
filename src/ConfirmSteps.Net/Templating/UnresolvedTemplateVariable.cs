namespace ConfirmSteps.Templating;

/// <summary>
/// A variable a request template expected, and where it was expected.
/// </summary>
/// <remarks>
/// The location is what makes the failure actionable: knowing that <c>SHOP_ID</c> is missing helps,
/// knowing it was missing from the request body rather than from a query parameter tells the author
/// which line of the descriptor to look at.
/// </remarks>
/// <param name="Name">Name of the variable, as the template spelled it.</param>
/// <param name="Location">
/// Part of the request that expected it: <c>base url</c>, <c>path segment 2</c>,
/// <c>query 'modelIds'</c>, <c>header 'Accept-language'</c>, <c>body</c> or <c>fragment</c>.
/// </param>
public sealed record UnresolvedTemplateVariable(string Name, string Location);
