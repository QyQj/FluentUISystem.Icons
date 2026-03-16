using System.Collections.Generic;

namespace FluentUISystem.Icons.WinUI3.Models
{
    public abstract class GradientPathFill : PathFillDefinition
    {
        public string SpreadMethod { get; set; } = string.Empty;

        public List<string> Transforms { get; set; } = new List<string>();

        public List<GradientStop> Stops { get; set; } = new List<GradientStop>();
    }
}
