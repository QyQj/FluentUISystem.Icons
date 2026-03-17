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

public partial class FluentUISystemIcon : MarkupExtension
{
    public Color Fill { get; set; } = Color.FromArgb(255, 33, 33, 33);

    public FluentSystemIconSymbol Symbol { get; set; }

    protected override object ProvideValue()
    {
        IconElement icon;
        var symbolStr = Symbol.ToString().ToLower();
        if (symbolStr.EndsWith("color"))
        {
            var size = Convert.ToInt32(symbolStr[^7..^5]);
            icon = new ImageIcon()
            {
                Width = size,
                Height = size,
                Source = new SvgImageSource(new Uri($"ms-appx:///FluentUISystem.Icons.WinUI3/Generated/ColorSvg/{Symbol}.svg"))
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

            icon = new PathIcon()
            {
                Width = svg.Width,
                Height = svg.Height,
                Data = group,
                Foreground = new SolidColorBrush(Fill)
            };
        }
        return icon;
    }
}