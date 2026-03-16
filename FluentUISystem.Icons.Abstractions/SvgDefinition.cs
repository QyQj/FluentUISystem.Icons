using System.Collections.Generic;

namespace FluentUISystem.Icons.Abstractions
{
    public class SvgDefinition
    {
        public string Id { get; set; }

        public double? Width { get; set; }

        public double? Height { get; set; }

        public List<PathDefinition> Paths { get; set; } = new List<PathDefinition>();
    }
}
