using System.Drawing;

namespace FluentUISystem.Icons.Abstractions.Models
{
    public class RadialGradientPathFill : GradientPathFill
    {
        public RadialGradientPathFill()
        {
        }

        public RadialGradientPathFill(string id)
        {
            FillId = id;
        }

        public PointF Center { get; set; }

        public PointF FocalPoint { get; set; }

        public double Radius { get; set; }
    }
}