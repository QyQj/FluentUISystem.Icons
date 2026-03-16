using System.Collections.Generic;
using System.Drawing;

namespace FluentUISystem.Icons.Abstractions
{
    public class RadialGradientPathFill : IGradientPathFill
    {
        public RadialGradientPathFill(string id)
        {
            FillId = id;
            Transforms = new List<string>();
            Stops = new List<GradientStop>();
        }

        public string FillId { get; }

        public PathFillType Type => PathFillType.RadialGradient;

        public List<string> Transforms { get; set; }

        public string SpreadMethod { get; set; }

        public List<GradientStop> Stops { get; set; }

        public PointF Center { get; set; }

        public PointF FocalPoint { get; set; }

        public float Radius { get; set; }
    }
}
