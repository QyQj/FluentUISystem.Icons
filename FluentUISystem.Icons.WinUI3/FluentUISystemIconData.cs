using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FluentUISystem.Icons.WinUI3.Models;

namespace FluentUISystem.Icons.WinUI3;

internal static class FluentUISystemIconData
{
    private const string ResourceName = "FluentUISystem.Icons.WinUI3.FluentUISystemIcon.g.json";
    private static readonly Lazy<IReadOnlyDictionary<string, SvgDefinition>> Definitions = new(LoadDefinitions);

    internal static SvgDefinition Get(string symbol)
    {
        if (Definitions.Value.TryGetValue(symbol, out var definition))
        {
            return definition;
        }

        throw new KeyNotFoundException($"Icon definition '{symbol}' was not found.");
    }

    private static IReadOnlyDictionary<string, SvgDefinition> LoadDefinitions()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Embedded icon data '{ResourceName}' was not found.");
        }

        var definitions = JsonSerializer.Deserialize<List<SvgDefinition>>(stream);
        if (definitions is null)
        {
            throw new InvalidOperationException("Failed to deserialize icon definitions.");
        }

        return definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
    }
}
