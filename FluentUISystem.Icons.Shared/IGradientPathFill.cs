using System.Collections.Generic;

namespace FluentUISystem.Icons.Shared
{
    public interface IGradientPathFill : IPathFill
    {
        List<string> Transforms { get; set; }

        string SpreadMethod { get; set; }

        List<GradientStop> Stops { get; set; }
    }
}