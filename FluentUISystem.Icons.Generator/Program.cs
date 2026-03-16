using FluentUISystem.Icons.Abstractions.Models;
using System.Globalization;
using System.Security;
using System.Text;

namespace FluentUISystem.Icons.Generator
{
    internal static class Program
    {
        private const string SymbolFileName = "FluentSystemIconSymbol.g.cs";
        private const string XamlRootFileName = "FluentSystemIconData.xaml";
        private const string XamlGroupFilePrefix = "FluentSystemIconData";
        private static readonly TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;

        private static void Main(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                throw new ArgumentException("Assets path is required.", nameof(args));
            }

            var assetsDirectory = new DirectoryInfo(args[0]);
            if (!assetsDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Assets path '{assetsDirectory.FullName}' does not exist.");
            }

            var iconDefinitions = CollectIconDefinitions(assetsDirectory);

            var sharedDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
#if X64
                "..",
#endif
                "..",
                "..",
                "..",
                "..",
                "FluentUISystem.Icons.Shared")));

            var winUiGeneratedDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
#if X64
                "..",
#endif
                "..",
                "..",
                "..",
                "..",
                "FluentUISystem.Icons.WinUI3",
                "Generated")));

            Directory.CreateDirectory(sharedDirectory.FullName);
            Directory.CreateDirectory(winUiGeneratedDirectory.FullName);

            CleanGeneratedOutputs(sharedDirectory, winUiGeneratedDirectory);

            File.WriteAllText(
                Path.Combine(sharedDirectory.FullName, SymbolFileName),
                BuildSymbolCode(iconDefinitions),
                new UTF8Encoding(false));

            var iconGroups = iconDefinitions
                .GroupBy(definition => GetGroupKey(definition.Id))
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .ToList();

            foreach (var iconGroup in iconGroups)
            {
                File.WriteAllText(
                    Path.Combine(winUiGeneratedDirectory.FullName, $"{XamlGroupFilePrefix}.{iconGroup.Key}.xaml"),
                    BuildGroupDictionaryXaml(iconGroup),
                    new UTF8Encoding(false));
            }

            File.WriteAllText(
                Path.Combine(winUiGeneratedDirectory.FullName, XamlRootFileName),
                BuildMergedDictionaryXaml(iconGroups.Select(group => group.Key).ToList()),
                new UTF8Encoding(false));
        }

        private static List<SvgDefinition> CollectIconDefinitions(DirectoryInfo assetsDirectory)
        {
            var iconDefinitions = new List<SvgDefinition>();
            var symbols = assetsDirectory.GetDirectories().OrderBy(directory => directory.Name, StringComparer.Ordinal);

            foreach (var symbol in symbols)
            {
                var baseSymbolName = symbol.Name.Replace(" ", string.Empty);
                if (symbol.Name == "Text Box Settings")
                {
                    baseSymbolName = "Text_BoxSettings";
                }

                var subSymbols = symbol.GetDirectories();
                if (subSymbols.Any(directory => directory.Name != "PDF" && directory.Name != "SVG"))
                {
                    foreach (var subSymbol in subSymbols.OrderBy(directory => directory.Name, StringComparer.Ordinal))
                    {
                        var lang = subSymbol.Name.Replace("-", string.Empty);
                        if (lang == "PDF")
                        {
                            continue;
                        }

                        if (lang == "SVG")
                        {
                            lang = string.Empty;
                        }

                        var svgFiles = subSymbol
                            .GetFiles("*.svg", SearchOption.AllDirectories)
                            .OrderBy(file => file.FullName, StringComparer.Ordinal);

                        iconDefinitions.AddRange(svgFiles.Select(file => ParseDefinition(baseSymbolName, file, lang)));
                    }
                }
                else
                {
                    var svgFiles = symbol
                        .GetFiles("*.svg", SearchOption.AllDirectories)
                        .OrderBy(file => file.FullName, StringComparer.Ordinal);

                    iconDefinitions.AddRange(svgFiles.Select(file => ParseDefinition(baseSymbolName, file)));
                }
            }

            return iconDefinitions
                .OrderBy(definition => definition.Id, StringComparer.Ordinal)
                .ToList();
        }

        private static void CleanGeneratedOutputs(DirectoryInfo sharedDirectory, DirectoryInfo winUiGeneratedDirectory)
        {
            foreach (var file in sharedDirectory.GetFiles("FluentSystemIconData.*.g.cs", SearchOption.TopDirectoryOnly))
            {
                file.Delete();
            }

            foreach (var file in winUiGeneratedDirectory.GetFiles($"{XamlGroupFilePrefix}*.xaml", SearchOption.TopDirectoryOnly))
            {
                file.Delete();
            }
        }

        private static SvgDefinition ParseDefinition(string baseSymbol, FileInfo svgFile, string lang = "")
        {
            var svgContent = File.ReadAllText(svgFile.FullName);
            var svgDefinition = SvgParser.Parse(svgContent);
            svgDefinition.Id = GetSymbolName(baseSymbol, svgFile, lang);
            return svgDefinition;
        }

        private static string GetSymbolName(string baseSymbol, FileInfo svgFile, string lang = "")
        {
            var nameParts = Path
                .GetFileNameWithoutExtension(svgFile.Name)
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => TextInfo.ToTitleCase(part))
                .ToList();

            if (nameParts.Count < 2)
            {
                throw new InvalidOperationException($"Unexpected svg file name format: '{svgFile.Name}'.");
            }

            if (nameParts[^1].Equals("ltr", StringComparison.OrdinalIgnoreCase) || nameParts[^1].Equals("rtl", StringComparison.OrdinalIgnoreCase))
            {
                baseSymbol += nameParts[^3];
            }

            return baseSymbol + nameParts[^2] + nameParts[^1] + TextInfo.ToTitleCase(lang);
        }

        private static string BuildSymbolCode(IReadOnlyList<SvgDefinition> iconDefinitions)
        {
            var builder = new StringBuilder();

            builder.AppendLine("namespace FluentUISystem.Icons.Shared;");
            builder.AppendLine();
            builder.AppendLine("public enum FluentSystemIconSymbol");
            builder.AppendLine("{");

            foreach (var icon in iconDefinitions)
            {
                builder.Append("    ");
                builder.Append(icon.Id);
                builder.AppendLine(",");
            }

            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string BuildMergedDictionaryXaml(IReadOnlyList<string> groupKeys)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<ResourceDictionary");
            builder.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            builder.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
            builder.AppendLine("    <ResourceDictionary.MergedDictionaries>");

            foreach (var groupKey in groupKeys)
            {
                builder.Append("        <ResourceDictionary Source=\"ms-appx:///Generated/");
                builder.Append(XamlGroupFilePrefix);
                builder.Append('.');
                builder.Append(groupKey);
                builder.AppendLine(".xaml\" />");
            }

            builder.AppendLine("    </ResourceDictionary.MergedDictionaries>");
            builder.AppendLine("</ResourceDictionary>");
            return builder.ToString();
        }

        private static string BuildGroupDictionaryXaml(IGrouping<string, SvgDefinition> iconGroup)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<ResourceDictionary");
            builder.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            builder.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            builder.AppendLine("    xmlns:abs=\"using:FluentUISystem.Icons.Abstractions.Models\"");
            builder.AppendLine("    xmlns:local=\"using:FluentUISystem.Icons.WinUI3\">");

            foreach (var icon in iconGroup.OrderBy(item => item.Id, StringComparer.Ordinal))
            {
                AppendSvgDefinition(builder, icon, 1);
            }

            builder.AppendLine("</ResourceDictionary>");
            return builder.ToString();
        }

        private static void AppendSvgDefinition(StringBuilder builder, SvgDefinition definition, int indentLevel)
        {
            AppendIndent(builder, indentLevel);
            builder.Append("<abs:SvgDefinition x:Key=\"");
            builder.Append(ToXmlAttribute(definition.Id));
            builder.Append("\" Id=\"");
            builder.Append(ToXmlAttribute(definition.Id));
            builder.Append('"');

            builder.Append(" Width=\"");
            builder.Append(ToXmlAttribute(ToDoubleLiteral(definition.Width)));
            builder.Append('"');

            builder.Append(" Height=\"");
            builder.Append(ToXmlAttribute(ToDoubleLiteral(definition.Height)));
            builder.Append('"');

            builder.AppendLine(">");
            AppendIndent(builder, indentLevel + 1);
            builder.AppendLine("<abs:SvgDefinition.Paths>");

            foreach (var path in definition.Paths)
            {
                AppendPathDefinition(builder, path, indentLevel + 2);
            }

            AppendIndent(builder, indentLevel + 1);
            builder.AppendLine("</abs:SvgDefinition.Paths>");
            AppendIndent(builder, indentLevel);
            builder.AppendLine("</abs:SvgDefinition>");
        }

        private static void AppendPathDefinition(StringBuilder builder, PathDefinition path, int indentLevel)
        {
            AppendIndent(builder, indentLevel);
            builder.Append("<abs:PathDefinition Data=\"");
            builder.Append(ToXmlAttribute(path.Data));
            builder.Append("\" FillOpacity=\"");
            builder.Append(ToXmlAttribute(ToDoubleLiteral(path.FillOpacity)));
            builder.Append('"');

            if (path.PathFill == null)
            {
                builder.AppendLine(" />");
                return;
            }

            builder.AppendLine(">");
            AppendIndent(builder, indentLevel + 1);
            builder.AppendLine("<abs:PathDefinition.PathFill>");
            AppendPathFill(builder, path.PathFill, indentLevel + 2);
            AppendIndent(builder, indentLevel + 1);
            builder.AppendLine("</abs:PathDefinition.PathFill>");
            AppendIndent(builder, indentLevel);
            builder.AppendLine("</abs:PathDefinition>");
        }

        private static void AppendPathFill(StringBuilder builder, PathFillDefinition pathFill, int indentLevel)
        {
            switch (pathFill)
            {
                case SolidPathFill solidFill:
                    AppendIndent(builder, indentLevel);
                    builder.Append("<abs:SolidPathFill FillId=\"");
                    builder.Append(ToXmlAttribute(solidFill.FillId));
                    builder.Append("\" Color=\"");
                    builder.Append(ToXmlAttribute(solidFill.Color));
                    builder.AppendLine("\" />");
                    return;

                case LinearGradientPathFill linearGradient:
                    AppendIndent(builder, indentLevel);
                    builder.Append("<abs:LinearGradientPathFill FillId=\"");
                    builder.Append(ToXmlAttribute(linearGradient.FillId));
                    builder.Append("\" SpreadMethod=\"");
                    builder.Append(ToXmlAttribute(linearGradient.SpreadMethod));
                    builder.Append("\" StartPoint=\"");
                    builder.Append(ToXmlAttribute(ToPointLiteral(linearGradient.StartPoint.X, linearGradient.StartPoint.Y)));
                    builder.Append("\" EndPoint=\"");
                    builder.Append(ToXmlAttribute(ToPointLiteral(linearGradient.EndPoint.X, linearGradient.EndPoint.Y)));
                    builder.AppendLine("\">");
                    AppendGradientChildren(builder, linearGradient.Transforms, linearGradient.Stops, indentLevel + 1);
                    AppendIndent(builder, indentLevel);
                    builder.AppendLine("</abs:LinearGradientPathFill>");
                    return;

                case RadialGradientPathFill radialGradient:
                    AppendIndent(builder, indentLevel);
                    builder.Append("<abs:RadialGradientPathFill FillId=\"");
                    builder.Append(ToXmlAttribute(radialGradient.FillId));
                    builder.Append("\" SpreadMethod=\"");
                    builder.Append(ToXmlAttribute(radialGradient.SpreadMethod));
                    builder.Append("\" Center=\"");
                    builder.Append(ToXmlAttribute(ToPointLiteral(radialGradient.Center.X, radialGradient.Center.Y)));
                    builder.Append("\" FocalPoint=\"");
                    builder.Append(ToXmlAttribute(ToPointLiteral(radialGradient.FocalPoint.X, radialGradient.FocalPoint.Y)));
                    builder.Append("\" Radius=\"");
                    builder.Append(ToXmlAttribute(ToDoubleLiteral(radialGradient.Radius)));
                    builder.AppendLine("\">");
                    AppendGradientChildren(builder, radialGradient.Transforms, radialGradient.Stops, indentLevel + 1);
                    AppendIndent(builder, indentLevel);
                    builder.AppendLine("</abs:RadialGradientPathFill>");
                    return;

                default:
                    throw new NotSupportedException($"Unsupported path fill type: {pathFill.GetType().FullName}");
            }
        }

        private static void AppendGradientChildren(StringBuilder builder, IEnumerable<string> transforms, IEnumerable<PathFillGradientStop> stops, int indentLevel)
        {
            AppendIndent(builder, indentLevel);
            builder.AppendLine("<abs:GradientPathFill.Transforms>");
            foreach (var transform in transforms)
            {
                AppendIndent(builder, indentLevel + 1);
                builder.Append("<x:String>");
                builder.Append(ToXmlText(transform));
                builder.AppendLine("</x:String>");
            }
            AppendIndent(builder, indentLevel);
            builder.AppendLine("</abs:GradientPathFill.Transforms>");

            AppendIndent(builder, indentLevel);
            builder.AppendLine("<abs:GradientPathFill.Stops>");
            foreach (var stop in stops)
            {
                AppendIndent(builder, indentLevel + 1);
                builder.Append("<abs:PathFillGradientStop Color=\"");
                builder.Append(ToXmlAttribute(stop.Color));
                builder.Append("\" Opacity=\"");
                builder.Append(ToXmlAttribute(ToDoubleLiteral(stop.Opacity)));
                builder.Append("\" Offset=\"");
                builder.Append(ToXmlAttribute(ToDoubleLiteral(stop.Offset)));
                builder.AppendLine("\" />");
            }
            AppendIndent(builder, indentLevel);
            builder.AppendLine("</abs:GradientPathFill.Stops>");
        }

        private static string GetGroupKey(string symbolId)
        {
            var firstCharacter = symbolId[0];
            return char.IsLetter(firstCharacter) ? char.ToUpperInvariant(firstCharacter).ToString() : "_";
        }

        private static void AppendIndent(StringBuilder builder, int indentLevel)
        {
            builder.Append(' ', indentLevel * 4);
        }

        private static string ToXmlAttribute(string? value)
        {
            return ToXmlText(value ?? string.Empty);
        }

        private static string ToXmlText(string value)
        {
            return SecurityElement.Escape(value) ?? string.Empty;
        }

        private static string ToDoubleLiteral(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string ToFloatLiteral(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string ToPointLiteral(float x, float y)
        {
            return $"{{local:PointFMarkupExt X = {ToFloatLiteral(x)},Y = {ToFloatLiteral(y)}}}";
        }
    }
}