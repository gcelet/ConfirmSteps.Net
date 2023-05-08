namespace ConfirmSteps.Templating;

using System.Text;
using System.Text.RegularExpressions;

public sealed class TemplateString : IEquatable<TemplateString>
{
    private static readonly Regex ExtractParamNames = new(@"{{\s*(?<paramName>[\w]+)\s*}}",
        RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    public TemplateString(string template)
    {
        Template = template;
    }

    public static TemplateString LoadFromFile(string path)
    {
        string fileContent = File.ReadAllText(path, Encoding.UTF8);

        return new TemplateString(fileContent);
    }

    public static bool operator ==(TemplateString? left, TemplateString? right)
    {
        return Equals(left, right);
    }

    public static implicit operator TemplateString(string template)
    {
        return new TemplateString(template);
    }

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