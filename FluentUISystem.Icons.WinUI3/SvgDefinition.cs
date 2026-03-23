using System.Collections.Generic;

namespace FluentUISystem.Icons.WinUI3;

internal sealed class SvgDefinition
{
    public string Id { get; set; } = string.Empty;

    public double Width { get; set; }

    public double Height { get; set; }

    public List<string> Paths { get; set; } = new List<string>();
}