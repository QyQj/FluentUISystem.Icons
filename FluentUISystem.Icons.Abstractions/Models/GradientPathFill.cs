using System.Collections.Generic;

namespace FluentUISystem.Icons.Abstractions.Models
{
    public abstract class GradientPathFill : PathFillDefinition
    {
        public string SpreadMethod { get; set; } = string.Empty;

        public List<string> Transforms { get; set; } = new List<string>();

        public List<PathFillGradientStop> Stops { get; set; } = new List<PathFillGradientStop>();
    }
}