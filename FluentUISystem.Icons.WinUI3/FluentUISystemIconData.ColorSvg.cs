using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace FluentUISystem.Icons.WinUI3;

internal static partial class FluentUISystemIconData
{
    private const string ColorSvgResourceName = "FluentUISystem.Icons.WinUI3.FluentUISystemIconData.ColorSvg.zip";
    private static readonly Lazy<IReadOnlyDictionary<string, byte[]>> ColorSvgContents = new(LoadColorSvgContents);

    internal static SvgImageSource CreateColorSvgImageSource(string symbol)
    {
        var imageSource = new SvgImageSource();
        _ = LoadArchivedColorSvgAsync(imageSource, symbol);
        return imageSource;
    }

    private static async Task LoadArchivedColorSvgAsync(SvgImageSource imageSource, string symbol)
    {
        try
        {
            var svgBytes = GetArchivedColorSvgBytes(symbol);
            using var memoryStream = new MemoryStream(svgBytes, writable: false);
            await imageSource.SetSourceAsync(memoryStream.AsRandomAccessStream());
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private static byte[] GetArchivedColorSvgBytes(string symbol)
    {
        if (ColorSvgContents.Value.TryGetValue(symbol, out var svgBytes))
        {
            return svgBytes;
        }

        throw new KeyNotFoundException($"Color icon '{symbol}' was not found.");
    }

    private static IReadOnlyDictionary<string, byte[]> LoadColorSvgContents()
    {
        using var archiveStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ColorSvgResourceName);
        if (archiveStream is null)
        {
            throw new InvalidOperationException($"Embedded color icon archive '{ColorSvgResourceName}' was not found.");
        }

        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: false);
        return archive.Entries
            .Where(static entry => entry.FullName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                static entry => Path.GetFileNameWithoutExtension(entry.FullName),
                static entry => ReadEntryBytes(entry),
                StringComparer.Ordinal);
    }

    private static byte[] ReadEntryBytes(ZipArchiveEntry entry)
    {
        using var entryStream = entry.Open();
        using var memoryStream = new MemoryStream();
        entryStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
