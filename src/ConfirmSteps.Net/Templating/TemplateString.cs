namespace ConfirmSteps.Templating;

using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a string that contains placeholders to be rendered with variable values.
/// </summary>
public sealed class TemplateString : IEquatable<TemplateString>
{
    private static readonly Regex ExtractParamNames = new(@"{{\s*(?<paramName>[\w]+)\s*}}",
        RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateString"/> class.
    /// </summary>
    /// <param name="template">The template string containing placeholders like {{variable}}.</param>
    public TemplateString(string template)
    {
        Template = template;
    }

    /// <summary>
    /// Loads a template string from a file.
    /// </summary>
    /// <param name="path">The path to the file containing the template.</param>
    /// <returns>A new <see cref="TemplateString"/> instance.</returns>
    public static TemplateString LoadFromFile(string path)
    {
        string fileContent = File.ReadAllText(path, Encoding.UTF8);

        return new TemplateString(fileContent);
    }

    /// <summary>
    /// Determines whether two <see cref="TemplateString"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="TemplateString"/>.</param>
    /// <param name="right">The second <see cref="TemplateString"/>.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(TemplateString? left, TemplateString? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="TemplateString"/>.
    /// </summary>
    /// <param name="template">The template string.</param>
    public static implicit operator TemplateString(string template)
    {
        return new TemplateString(template);
    }

    /// <summary>
    /// Determines whether two <see cref="TemplateString"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="TemplateString"/>.</param>
    /// <param name="right">The second <see cref="TemplateString"/>.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(TemplateString? left, TemplateString? right)
    {
        return !Equals(left, right);
    }

    private string Template { get; }

    /// <inheritdoc />
    public bool Equals(TemplateString? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Template == other.Template;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is TemplateString other && Equals(other));
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Template.GetHashCode();
    }

    /// <summary>
    /// Renders the template string by replacing placeholders with values from the provided dictionary.
    /// </summary>
    /// <param name="vars">A dictionary containing the variable values.</param>
    /// <returns>The rendered string.</returns>
    public string Render(IReadOnlyDictionary<string, object> vars)
    {
        Match match = ExtractParamNames.Match(Template);

        if (!match.Success)
        {
            return Template;
        }

        int index = 0;
        StringBuilder sb = new();

        do
        {
            if (match.Index > index)
            {
                sb.Append(Template.Substring(index, match.Index - index));
            }

            Group group = match.Groups["paramName"];
            string paramName = group.Value;

            if (vars.TryGetValue(paramName, out object? value) && value != null)
            {
                sb.Append(value);
            }
            else
            {
                sb.Append(match.Value);
            }

            index = match.Index + match.Value.Length;

            match = match.NextMatch();
        } while (match.Success);

        if (index < Template.Length)
        {
            sb.Append(Template.Substring(index));
        }

        return sb.ToString();
    }
}
