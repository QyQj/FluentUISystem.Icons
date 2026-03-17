using System.Globalization;
using System.Security;
using System.Text;
using FluentUISystem.Icons.Abstractions.Models;

namespace FluentUISystem.Icons.Generator
{
    internal static class Program
    {
        private const string SymbolFileName = "FluentSystemIconSymbol.g.cs";
        private const string XamlRootFileName = "FluentSystemIconData.xaml";
        private const string XamlGroupFilePrefix = "FluentSystemIconData";
        private const string ColorSvgDirectoryName = "ColorSvg";
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

            var iconAssets = CollectIconAssets(assetsDirectory);

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
            var colorSvgDirectory = new DirectoryInfo(Path.Combine(winUiGeneratedDirectory.FullName, ColorSvgDirectoryName));

            Directory.CreateDirectory(sharedDirectory.FullName);
            Directory.CreateDirectory(winUiGeneratedDirectory.FullName);
            Directory.CreateDirectory(colorSvgDirectory.FullName);

            CleanGeneratedOutputs(sharedDirectory, winUiGeneratedDirectory, colorSvgDirectory);

            File.WriteAllText(
                Path.Combine(sharedDirectory.FullName, SymbolFileName),
                BuildSymbolCode(iconAssets.Select(asset => asset.SymbolName).ToList()),
                new UTF8Encoding(false));

            var xamlDefinitions = iconAssets
                .Where(asset => !asset.IsColor && asset.Definition != null)
                .Select(asset => asset.Definition!)
                .ToList();

            File.WriteAllText(
                Path.Combine(winUiGeneratedDirectory.FullName, $"{XamlGroupFilePrefix}.xaml"),
                BuildDictionaryXaml(xamlDefinitions),
                new UTF8Encoding(false));

            foreach (var colorAsset in iconAssets.Where(asset => asset.IsColor))
            {
                File.Copy(colorAsset.SvgFile.FullName, Path.Combine(colorSvgDirectory.FullName, colorAsset.SymbolName + ".svg"), true);
            }
        }

        private static List<IconAsset> CollectIconAssets(DirectoryInfo assetsDirectory)
        {
            var iconAssets = new List<IconAsset>();
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

                        iconAssets.AddRange(svgFiles.Select(file => CreateIconAsset(baseSymbolName, file, lang)));
                    }
                }
                else
                {
                    var svgFiles = symbol
                        .GetFiles("*.svg", SearchOption.AllDirectories)
                        .OrderBy(file => file.FullName, StringComparer.Ordinal);

                    iconAssets.AddRange(svgFiles.Select(file => CreateIconAsset(baseSymbolName, file)));
                }
            }

            return iconAssets
                .OrderBy(asset => asset.SymbolName, StringComparer.Ordinal)
                .ToList();
        }

        private static void CleanGeneratedOutputs(DirectoryInfo sharedDirectory, DirectoryInfo winUiGeneratedDirectory, DirectoryInfo colorSvgDirectory)
        {
            foreach (var file in sharedDirectory.GetFiles("FluentSystemIconData.*.g.cs", SearchOption.TopDirectoryOnly))
            {
                file.Delete();
            }

            foreach (var file in winUiGeneratedDirectory.GetFiles($"{XamlGroupFilePrefix}*.xaml", SearchOption.TopDirectoryOnly))
            {
                file.Delete();
            }

            foreach (var file in colorSvgDirectory.GetFiles("*.svg", SearchOption.TopDirectoryOnly))
            {
                file.Delete();
            }
        }

        private static IconAsset CreateIconAsset(string baseSymbol, FileInfo svgFile, string lang = "")
        {
            var symbolName = GetSymbolName(baseSymbol, svgFile, lang);
            if (IsColorSvg(svgFile))
            {
                return new IconAsset(symbolName, svgFile, null, true);
            }

            var svgContent = File.ReadAllText(svgFile.FullName);
            var svgDefinition = SvgParser.Parse(svgContent);
            svgDefinition.Id = symbolName;
            return new IconAsset(symbolName, svgFile, svgDefinition, false);
        }

        private static bool IsColorSvg(FileInfo svgFile)
        {
            return Path.GetFileNameWithoutExtension(svgFile.Name).EndsWith("color", StringComparison.OrdinalIgnoreCase);
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

        private static string BuildSymbolCode(IReadOnlyList<string> symbolNames)
        {
            var builder = new StringBuilder();

            builder.AppendLine("namespace FluentUISystem.Icons.Shared;");
            builder.AppendLine();
            builder.AppendLine("public enum FluentSystemIconSymbol");
            builder.AppendLine("{");

            foreach (var symbolName in symbolNames)
            {
                builder.Append("    ");
                builder.Append(symbolName);
                builder.AppendLine(",");
            }

            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string BuildDictionaryXaml(IList<SvgDefinition> svgs)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<ResourceDictionary");
            builder.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            builder.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            builder.AppendLine("    xmlns:abs=\"using:FluentUISystem.Icons.Abstractions.Models\"");
            builder.AppendLine("    xmlns:local=\"using:FluentUISystem.Icons.WinUI3\">");

            foreach (var icon in svgs.OrderBy(item => item.Id, StringComparer.Ordinal))
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

            builder.Append("\" Width=\"");
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
            builder.Append("\" Fill=\"");
            builder.Append(ToXmlAttribute(path.Fill));
            builder.Append("\" FillOpacity=\"");
            builder.Append(ToXmlAttribute(ToDoubleLiteral(path.FillOpacity)));
            builder.AppendLine("\"/>");
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

        private sealed class IconAsset
        {
            public IconAsset(string symbolName, FileInfo svgFile, SvgDefinition? definition, bool isColor)
            {
                SymbolName = symbolName;
                SvgFile = svgFile;
                Definition = definition;
                IsColor = isColor;
            }

            public string SymbolName { get; }

            public FileInfo SvgFile { get; }

            public SvgDefinition? Definition { get; }

            public bool IsColor { get; }
        }
    }
}