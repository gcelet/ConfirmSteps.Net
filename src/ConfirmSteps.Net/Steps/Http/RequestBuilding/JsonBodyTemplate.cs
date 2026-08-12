namespace ConfirmSteps.Steps.Http.RequestBuilding;

using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

using ConfirmSteps.Templating;

/// <summary>
/// Renders a request body described as a JSON structure whose values may be variables.
/// </summary>
/// <remarks>
/// <para>
/// The template is ordinary JSON — no type of its own to learn, and a body read out of a file passes
/// straight through. What makes it a template is that a string value may be a placeholder.
/// </para>
/// <para>
/// The body is <b>built and serialised</b>, never assembled as text. That is the whole reason to
/// describe it as a structure: escaping is guaranteed by the serialiser rather than by whoever wrote
/// the file, and a value carrying a quote, a backslash or a newline cannot break the document — nor
/// be used to inject one.
/// </para>
/// </remarks>
internal static class JsonBodyTemplate
{
    /// <summary>
    /// The template with its variables substituted.
    /// </summary>
    /// <remarks>
    /// A string value that is <b>exactly one placeholder</b> takes the type of the variable: a number
    /// stays a number, a collection becomes an array, an object becomes an object. The quotes around
    /// it in the template are how a placeholder is written inside JSON, not a claim that the result is
    /// a string. A string value with a placeholder <b>inside surrounding text</b> is rendered as text,
    /// as everywhere else.
    /// <para>
    /// Property names are literal: a name is not a template. Nothing needs a computed property name,
    /// and refusing to substitute there keeps a descriptor readable as the document it produces.
    /// </para>
    /// </remarks>
    /// <param name="template">The body template.</param>
    /// <param name="vars">The variables of the current step.</param>
    public static JsonNode? Render(JsonNode? template, IReadOnlyDictionary<string, object> vars)
    {
        switch (template)
        {
            case null:
            {
                return null;
            }

            case JsonObject templateObject:
            {
                JsonObject rendered = new();

                foreach (KeyValuePair<string, JsonNode?> property in templateObject)
                {
                    if (OptionalPropertyName(property.Key) is { } name)
                    {
                        // Absent means ABSENT: the property is left out of the document rather than
                        // sent empty or null. For many endpoints those are three different requests.
                        if (HasEveryVariable(property.Value, vars))
                        {
                            rendered[name] = Render(property.Value, vars);
                        }

                        continue;
                    }

                    rendered[property.Key] = Render(property.Value, vars);
                }

                return rendered;
            }

            case JsonArray templateArray:
            {
                JsonArray rendered = new();

                foreach (JsonNode? element in templateArray)
                {
                    rendered.Add(Render(element, vars));
                }

                return rendered;
            }

            default:
            {
                return RenderValue(template, vars);
            }
        }
    }

    /// <summary>
    /// Collects the variables the template expects but the step does not have.
    /// </summary>
    /// <remarks>
    /// Walked separately from rendering so every missing variable is reported at once, with the place
    /// in the document that wanted it — <c>body $.customer.shopId</c> rather than just <c>body</c>.
    /// </remarks>
    /// <param name="template">The body template.</param>
    /// <param name="vars">The variables of the current step.</param>
    /// <param name="path">JSON path of the node being visited.</param>
    /// <param name="unresolved">Collects what is missing.</param>
    public static void CollectUnresolved(JsonNode? template, IReadOnlyDictionary<string, object> vars,
        string path, ICollection<UnresolvedTemplateVariable> unresolved)
    {
        switch (template)
        {
            case JsonObject templateObject:
            {
                foreach (KeyValuePair<string, JsonNode?> property in templateObject)
                {
                    // An optional property has nothing to report: not having a value is what it is
                    // for, and it will simply not be part of the document.
                    if (OptionalPropertyName(property.Key) is not null)
                    {
                        continue;
                    }

                    CollectUnresolved(property.Value, vars, $"{path}.{property.Key}", unresolved);
                }

                break;
            }

            case JsonArray templateArray:
            {
                for (int i = 0; i < templateArray.Count; i++)
                {
                    CollectUnresolved(templateArray[i], vars, $"{path}[{i}]", unresolved);
                }

                break;
            }

            case JsonValue value when TryReadTemplate(value, out TemplateString? asTemplate):
            {
                foreach (string name in asTemplate.ParameterNames)
                {
                    if (!vars.TryGetValue(name, out object? bound) || bound == null)
                    {
                        unresolved.Add(new UnresolvedTemplateVariable(name, $"body {path}"));
                    }
                }

                break;
            }

            default:
            {
                break;
            }
        }
    }

    private static JsonNode? RenderValue(JsonNode template, IReadOnlyDictionary<string, object> vars)
    {
        if (template is not JsonValue value || !TryReadTemplate(value, out TemplateString? asTemplate))
        {
            // A number, a boolean or a plain string in the template: copied as it stands. Round-tripped
            // rather than DeepClone'd, which only exists from .NET 8 and this library also targets 6.
            return JsonNode.Parse(template.ToJsonString());
        }

        if (!asTemplate.IsSinglePlaceholder)
        {
            string rendered = asTemplate.Render(vars);

            // A collection cannot be interpolated into text: it used to serialise as its type name.
            if (asTemplate.ParameterNames.Count == 1
                && vars.TryGetValue(asTemplate.ParameterNames[0], out object? embedded)
                && embedded is not (null or string) and IEnumerable)
            {
                throw new MultiValuedTemplateVariableException(asTemplate.ParameterNames[0]);
            }

            return JsonValue.Create(rendered);
        }

        object? bound = vars.TryGetValue(asTemplate.ParameterNames[0], out object? found) ? found : null;

        // Through the serialiser rather than by hand: it types numbers and booleans, turns a
        // collection into an array and an object into an object, and formats every number the same
        // way whatever the machine's culture — which string interpolation does not.
        return bound == null
            ? null
            : JsonSerializer.SerializeToNode(bound, HttpSettings.BuildJsonSerializerOptions());
    }

    /// <summary>
    /// Whether a property is declared optional, and its name without the marker.
    /// </summary>
    /// <remarks>
    /// A trailing question mark on the <b>key</b> — <c>"searchText?"</c> — reads the way an optional
    /// member reads in TypeScript, which is where the people writing these documents see it every day.
    /// It also keeps the template plain JSON: no reserved object shape to learn, and nothing to change
    /// in the templating engine, which the reporting templates share.
    /// </remarks>
    private static string? OptionalPropertyName(string key)
        => key.Length > 1 && key[^1] == '?' ? key[..^1] : null;

    /// <summary>
    /// Whether every variable a template node expects has a value.
    /// </summary>
    /// <remarks>
    /// What decides whether an optional property is part of the document. A node built from several
    /// placeholders needs all of them: half a sentence is not a value worth sending.
    /// </remarks>
    private static bool HasEveryVariable(JsonNode? template, IReadOnlyDictionary<string, object> vars)
    {
        List<UnresolvedTemplateVariable> unresolved = new();

        CollectUnresolved(template, vars, "$", unresolved);

        return unresolved.Count == 0;
    }

    private static bool TryReadTemplate(JsonValue value, out TemplateString asTemplate)
    {
        if (value.TryGetValue(out string? text) && text != null)
        {
            asTemplate = new TemplateString(text);

            return asTemplate.ParameterNames.Count > 0;
        }

        asTemplate = new TemplateString(string.Empty);

        return false;
    }
}
