using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using System.Drawing;

namespace FluentUISystem.Icons.WinUI3
{
    public class PointFMarkupExt : MarkupExtension
    {
        public double X { get; set; }

        public double Y { get; set; }

        protected override object ProvideValue(IXamlServiceProvider serviceProvider)
        {
            return new PointF((float)X, (float)Y);
        }
    }
}