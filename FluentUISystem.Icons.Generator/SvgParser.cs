using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FluentUISystem.Icons.Generator;

internal sealed class SvgParser
{
    private static readonly Regex TransformRegex = new Regex(@"[a-zA-Z]+\([^)]*\)", RegexOptions.Compiled);

    internal static SvgDefinition Parse(string svgContent)
    {
        if (string.IsNullOrWhiteSpace(svgContent))
        {
            throw new ArgumentException("SVG content cannot be null or empty.", nameof(svgContent));
        }

        var document = XDocument.Parse(svgContent, LoadOptions.PreserveWhitespace);
        return Parse(document);
    }

    internal static SvgDefinition Parse(XDocument document)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        var root = document.Root;
        if (root == null || !string.Equals(root.Name.LocalName, "svg", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The provided document is not a valid SVG.");
        }

        var result = new SvgDefinition
        {
            Width = ParseLength(GetAttribute(root, "width")),
            Height = ParseLength(GetAttribute(root, "height")),
        };

        foreach (var pathElement in root.Descendants().Where(element => string.Equals(element.Name.LocalName, "path", StringComparison.OrdinalIgnoreCase)))
        {
            result.Paths.Add(ParsePath(pathElement));
        }

        if (result.Paths.Count > 1)
        {
        }

        return result;
    }

    private static PathDefinition ParsePath(XElement pathElement)
    {
        var fillValue = GetAttribute(pathElement, "fill");

        return new PathDefinition
        {
            Data = GetAttribute(pathElement, "d"),
            FillOpacity = ParseDouble(GetAttribute(pathElement, "fill-opacity")) ?? 1d,
            Fill = fillValue
        };
    }

    private static string GetAttribute(XElement element, string name)
    {
        return element
            .Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, name, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    private static double? ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.EndsWith("%", StringComparison.Ordinal))
        {
            normalized = normalized.Substring(0, normalized.Length - 1);
            if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var percentage))
            {
                return percentage / 100d;
            }

            return null;
        }

        if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }

    private static double ParseLength(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        var numeric = new string(value.Trim().TakeWhile(character => char.IsDigit(character) || character == '.' || character == '-' || character == '+').ToArray());
        if (string.IsNullOrWhiteSpace(numeric))
        {
            return 0;
        }

        if (double.TryParse(numeric, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return 0;
    }
}