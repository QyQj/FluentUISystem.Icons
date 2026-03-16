using System.Collections.Generic;

namespace FluentUISystem.Icons.Abstractions
{
    public interface IGradientPathFill : IPathFill
    {
        List<string> Transforms { get; set; }

        string SpreadMethod { get; set; }

        List<GradientStop> Stops { get; set; }
    }
}