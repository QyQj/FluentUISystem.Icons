using System.Collections.Generic;
using System.Drawing;

namespace FluentUISystem.Icons.Abstractions
{
    public class LinearGradientPathFill : IGradientPathFill
    {
        public LinearGradientPathFill(string id)
        {
            FillId = id;
            Transforms = new List<string>();
            Stops = new List<GradientStop>();
        }

        public string FillId { get; }

        public PathFillType Type => PathFillType.LinearGradient;

        public List<string> Transforms { get; set; }

        public string SpreadMethod { get; set; }

        public List<GradientStop> Stops { get; set; }

        public PointF StartPoint { get; set; }

        public PointF EndPoint { get; set; }
    }
}
