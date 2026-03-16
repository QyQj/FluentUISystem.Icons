using System.Collections.Generic;
using System.Drawing;

namespace FluentUISystem.Icons.Shared
{
    public class LinearGradientPathFill : IGradientPathFill
    {
        public string Id { get; }

        public PathFillType Type => PathFillType.LinearGradient;

        public double Opacity { get; set; }

        public List<string> Transforms { get; set; }

        public string SpreadMethod { get; set; }

        public List<GradientStop> Stops { get; set; }

        public PointF StartPoint { get; set; }
        public PointF EndPoint { get; set; }
    }
}