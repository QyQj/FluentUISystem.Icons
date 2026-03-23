using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace FluentUISystem.Icons.WinUI3;

internal static partial class FluentUISystemIconData
{
    private const string PathSvgResourceName = "FluentUISystem.Icons.WinUI3.FluentUISystemIconData.PathSvg.json";
    private static readonly Lazy<IReadOnlyDictionary<string, SvgDefinition>> PathSvgDefinitions = new(LoadPathSvgDefinitions);

    internal static SvgDefinition GetPathSvgDefinition(string symbol)
    {
        if (PathSvgDefinitions.Value.TryGetValue(symbol, out var definition))
        {
            return definition;
        }

        throw new KeyNotFoundException($"Icon definition '{symbol}' was not found.");
    }

    private static IReadOnlyDictionary<string, SvgDefinition> LoadPathSvgDefinitions()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(PathSvgResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Embedded icon data '{PathSvgResourceName}' was not found.");
        }

        var definitions = JsonSerializer.Deserialize<List<SvgDefinition>>(stream);
        if (definitions is null)
        {
            throw new InvalidOperationException("Failed to deserialize icon definitions.");
        }

        return definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
    }
}