using System;
using FluentUISystem.Icons.Abstractions.Models;
using FluentUISystem.Icons.Shared;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace FluentUISystem.Icons.WinUI3;

public partial class FluentUISystemIconSource : MarkupExtension
{
    public Color Fill { get; set; } = Color.FromArgb(255, 33, 33, 33);

    public FluentSystemIconSymbol Symbol { get; set; }

    protected override object ProvideValue()
    {
        IconSource source;
        if (Symbol.ToString().ToLower().EndsWith("color"))
        {
            source = new ImageIconSource()
            {
                ImageSource = new SvgImageSource(new Uri($"ms-appx:///FluentUISystem.Icons.WinUI3/Generated/ColorSvg/{Symbol}.svg"))
            };
        }
        else
        {
            var svg = Application.Current.Resources[Symbol.ToString()] as SvgDefinition;
            ArgumentNullException.ThrowIfNull(svg);
            var group = new GeometryGroup();
            foreach (var pathDefinition in svg.Paths)
            {
                group.Children.Add(Utils.PathMarkupToGeometry(pathDefinition.Data));
            }

            source = new PathIconSource
            {
                Data = group,
                Foreground = new SolidColorBrush(Fill)
            };
        }
        return source;
    }
}