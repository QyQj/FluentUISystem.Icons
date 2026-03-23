using System.Globalization;
using System.Text;
using System.Text.Json;

namespace FluentUISystem.Icons.Generator;

internal static class Program
{
    private const string SymbolFileName = "FluentUISystemIconSymbol.g.cs";
    private const string JsonFileName = "FluentUISystemIcon.g.json";
    private const string ColorSvgDirectoryName = "ColorSvg";
    private static readonly TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

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

        var outputDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
#if X64
            "..",
#endif
            "..",
            "..",
            "..",
            "..",
            "FluentUISystem.Icons.WinUI3")));
        var colorSvgDirectory = new DirectoryInfo(Path.Combine(outputDirectory.FullName, "Assets", ColorSvgDirectoryName));

        Directory.CreateDirectory(colorSvgDirectory.FullName);

        File.WriteAllText(
            Path.Combine(outputDirectory.FullName, SymbolFileName),
            BuildSymbolCode(iconAssets.Select(asset => asset.SymbolName).ToList()),
            new UTF8Encoding(false));

        var xamlDefinitions = iconAssets
            .Where(asset => asset is { IsColor: false, Definition: not null })
            .Select(asset => asset.Definition!)
            .ToList();

        File.WriteAllText(
            Path.Combine(outputDirectory.FullName, JsonFileName),
            JsonSerializer.Serialize(xamlDefinitions, JsonOptions),
            new UTF8Encoding(false));

        foreach (var file in colorSvgDirectory.GetFiles())
        {
            file.Delete();
        }

        var count = 0;
        var colorIcons = iconAssets.Where(asset => asset.IsColor).ToList();
        foreach (var colorAsset in colorIcons)
        {
            var dest = Path.Combine(colorSvgDirectory.FullName, colorAsset.SymbolName + ".svg");
            Console.WriteLine($"{++count}/{colorIcons.Count} Copying {colorAsset.SvgFile.Name} to {dest})");
            File.Copy(colorAsset.SvgFile.FullName, dest, true);
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

        builder.AppendLine("namespace FluentUISystem.Icons.WinUI3;");
        builder.AppendLine();
        builder.AppendLine("public enum FluentUISystemIconSymbol");
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

    private sealed class IconAsset
    {
        internal IconAsset(string symbolName, FileInfo svgFile, SvgDefinition? definition, bool isColor)
        {
            SymbolName = symbolName;
            SvgFile = svgFile;
            Definition = definition;
            IsColor = isColor;
        }

        internal string SymbolName { get; }

        internal FileInfo SvgFile { get; }

        internal SvgDefinition? Definition { get; }

        internal bool IsColor { get; }
    }
}
