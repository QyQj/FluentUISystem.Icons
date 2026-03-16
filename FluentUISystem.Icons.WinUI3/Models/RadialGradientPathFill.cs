using Windows.Foundation;

namespace FluentUISystem.Icons.WinUI3.Models
{
    public class RadialGradientPathFill : GradientPathFill
    {
        public Point Center { get; set; }

        public Point FocalPoint { get; set; }

        public double Radius { get; set; }
    }
}
