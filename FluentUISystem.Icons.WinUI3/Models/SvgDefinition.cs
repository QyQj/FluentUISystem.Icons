using System.Collections.Generic;

namespace FluentUISystem.Icons.WinUI3.Models
{
    public class SvgDefinition
    {
        public string Id { get; set; } = string.Empty;

        public double? Width { get; set; }

        public double? Height { get; set; }

        public List<PathDefinition> Paths { get; set; } = new List<PathDefinition>();
    }
}
