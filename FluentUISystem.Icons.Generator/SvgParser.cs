using FluentUISystem.Icons.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FluentUISystem.Icons.Generator
{
    public sealed class SvgParser
    {
        private static readonly Regex TransformRegex = new Regex(@"[a-zA-Z]+\([^)]*\)", RegexOptions.Compiled);

        public static SvgDefinition Parse(string svgContent)
        {
            if (string.IsNullOrWhiteSpace(svgContent))
            {
                throw new ArgumentException("SVG content cannot be null or empty.", nameof(svgContent));
            }

            var document = XDocument.Parse(svgContent, LoadOptions.PreserveWhitespace);
            return Parse(document);
        }

        public static SvgDefinition Parse(XDocument document)
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

            var fillDefinitions = ParseFillDefinitions(root).ToDictionary(fill => fill.FillId, StringComparer.Ordinal);

            foreach (var pathElement in root.Descendants().Where(element => string.Equals(element.Name.LocalName, "path", StringComparison.OrdinalIgnoreCase)))
            {
                result.Paths.Add(ParsePath(pathElement, fillDefinitions));
            }

            return result;
        }

        private static IReadOnlyList<PathFillDefinition> ParseFillDefinitions(XElement root)
        {
            var fills = new List<PathFillDefinition>();

            foreach (var defs in root.Elements().Where(element => string.Equals(element.Name.LocalName, "defs", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var element in defs.Elements())
                {
                    if (string.Equals(element.Name.LocalName, "linearGradient", StringComparison.OrdinalIgnoreCase))
                    {
                        fills.Add(ParseLinearGradient(element));
                    }
                    else if (string.Equals(element.Name.LocalName, "radialGradient", StringComparison.OrdinalIgnoreCase))
                    {
                        fills.Add(ParseRadialGradient(element));
                    }
                }
            }

            return fills;
        }

        private static PathDefinition ParsePath(XElement pathElement, IReadOnlyDictionary<string, PathFillDefinition> fillDefinitions)
        {
            var fillValue = GetAttribute(pathElement, "fill");

            return new PathDefinition
            {
                Data = GetAttribute(pathElement, "d"),
                FillOpacity = ParseDouble(GetAttribute(pathElement, "fill-opacity")) ?? 1d,
                PathFill = ResolveFill(fillValue, fillDefinitions)
            };
        }

        private static PathFillDefinition ResolveFill(string fillValue, IReadOnlyDictionary<string, PathFillDefinition> fillDefinitions)
        {
            if (string.IsNullOrWhiteSpace(fillValue) ||
                string.Equals(fillValue, "none", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var fillId = TryExtractUrlId(fillValue);
            if (!string.IsNullOrEmpty(fillId))
            {
                if (fillDefinitions.TryGetValue(fillId, out var fill))
                {
                    return fill;
                }

                throw new InvalidOperationException($"Referenced fill definition '{fillId}' was not found.");
            }

            return new SolidPathFill
            {
                Color = fillValue
            };
        }

        private static LinearGradientPathFill ParseLinearGradient(XElement element)
        {
            var gradient = new LinearGradientPathFill(GetRequiredAttribute(element, "id"))
            {
                SpreadMethod = GetAttribute(element, "spreadMethod"),
                StartPoint = new PointF(
                    ParseFloat(GetAttribute(element, "x1")) ?? 0f,
                    ParseFloat(GetAttribute(element, "y1")) ?? 0f),
                EndPoint = new PointF(
                    ParseFloat(GetAttribute(element, "x2")) ?? 1f,
                    ParseFloat(GetAttribute(element, "y2")) ?? 0f)
            };

            gradient.Transforms = ParseTransforms(GetAttribute(element, "gradientTransform"));
            gradient.Stops = ParseStops(element);
            return gradient;
        }

        private static RadialGradientPathFill ParseRadialGradient(XElement element)
        {
            var centerX = ParseFloat(GetAttribute(element, "cx")) ?? 0.5f;
            var centerY = ParseFloat(GetAttribute(element, "cy")) ?? 0.5f;

            var gradient = new RadialGradientPathFill(GetRequiredAttribute(element, "id"))
            {
                SpreadMethod = GetAttribute(element, "spreadMethod"),
                Center = new PointF(centerX, centerY),
                FocalPoint = new PointF(
                    ParseFloat(GetAttribute(element, "fx")) ?? centerX,
                    ParseFloat(GetAttribute(element, "fy")) ?? centerY),
                Radius = ParseFloat(GetAttribute(element, "r")) ?? 0.5f
            };

            gradient.Transforms = ParseTransforms(GetAttribute(element, "gradientTransform"));
            gradient.Stops = ParseStops(element);
            return gradient;
        }

        private static List<PathFillGradientStop> ParseStops(XElement gradientElement)
        {
            return gradientElement
                .Elements()
                .Where(element => string.Equals(element.Name.LocalName, "stop", StringComparison.OrdinalIgnoreCase))
                .Select(element => new PathFillGradientStop
                {
                    Color = GetAttribute(element, "stop-color"),
                    Opacity = ParseDouble(GetAttribute(element, "stop-opacity")) ?? 1d,
                    Offset = ParseOffset(GetAttribute(element, "offset"))
                })
                .ToList();
        }

        private static List<string> ParseTransforms(string transformValue)
        {
            if (string.IsNullOrWhiteSpace(transformValue))
            {
                return new List<string>();
            }

            return TransformRegex.Matches(transformValue)
                .Cast<Match>()
                .Select(match => match.Value)
                .ToList();
        }

        private static string TryExtractUrlId(string fillValue)
        {
            const string prefix = "url(#";
            if (fillValue.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && fillValue.EndsWith(")", StringComparison.Ordinal))
            {
                return fillValue.Substring(prefix.Length, fillValue.Length - prefix.Length - 1);
            }

            return null;
        }

        private static string GetRequiredAttribute(XElement element, string name)
        {
            var value = GetAttribute(element, name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Element '{element.Name.LocalName}' is missing required attribute '{name}'.");
            }

            return value;
        }

        private static string GetAttribute(XElement element, string name)
        {
            return element.Attributes().FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, name, StringComparison.OrdinalIgnoreCase))?.Value;
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

        private static float? ParseFloat(string value)
        {
            var number = ParseDouble(value);
            return number.HasValue ? (float?)number.Value : null;
        }

        private static double ParseOffset(string value)
        {
            return ParseDouble(value) ?? 0d;
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
}