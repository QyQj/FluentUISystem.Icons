using System.Collections.Generic;

namespace FluentUISystem.Icons.WinUI3.Models;

internal class SvgDefinition
{
    internal string Id { get; set; } = string.Empty;

    internal double Width { get; set; }

    internal double Height { get; set; }

    internal List<PathDefinition> Paths { get; set; }=new List<PathDefinition>();
}