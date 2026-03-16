using System.Drawing;

namespace FluentUISystem.Icons.Abstractions.Models
{
    public class LinearGradientPathFill : GradientPathFill
    {
        public LinearGradientPathFill()
        {
        }

        public LinearGradientPathFill(string id)
        {
            FillId = id;
        }

        public PointF StartPoint { get; set; }

        public PointF EndPoint { get; set; }
    }
}