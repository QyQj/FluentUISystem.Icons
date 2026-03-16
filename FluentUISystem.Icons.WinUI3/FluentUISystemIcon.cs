using FluentUISystem.Icons.Abstractions.Models;
using FluentUISystem.Icons.Shared;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.UI;

namespace FluentUISystem.Icons.WinUI3;

public class FluentUISystemIcon : MarkupExtension
{
    private static readonly Regex TranslateRegex = new Regex(@"translate\(\s*([-+]?\d*\.?\d+)\s+([-+]?\d*\.?\d+)\s*\)", RegexOptions.Compiled);
    private static readonly Regex RotateRegex = new Regex(@"rotate\(\s*([-+]?\d*\.?\d+)\s*\)", RegexOptions.Compiled);
    private static readonly Regex ScaleRegex = new Regex(@"scale\(\s*([-+]?\d*\.?\d+)\s+([-+]?\d*\.?\d+)\s*\)", RegexOptions.Compiled);

    public FluentSystemIconSymbol Symbol { get; set; }

    protected override object ProvideValue()
    {
        var svg = Application.Current.Resources[Symbol.ToString()] as SvgDefinition;
        Grid grid = new Grid
        {
            Width = svg!.Width,
            Height = svg!.Height
        };
        foreach (var pathDefinition in svg.Paths)
        {
            var data = PathMarkupToGeometry(pathDefinition.Data);
            Brush brush;
            switch (pathDefinition.PathFill)
            {
                case SolidPathFill solidPathFill:
                    brush = new SolidColorBrush(ConvertToColor(solidPathFill.Color, pathDefinition.FillOpacity));
                    break;

                case LinearGradientPathFill linearGradientPathFill:
                    brush = new LinearGradientBrush();
                    var linearGradientBrush = brush as LinearGradientBrush;
                    linearGradientBrush!.StartPoint = new Point(linearGradientPathFill.StartPoint.X / svg.Width, linearGradientPathFill.StartPoint.Y / svg.Height);
                    linearGradientBrush.EndPoint = new Point(linearGradientPathFill.EndPoint.X / svg.Width, linearGradientPathFill.EndPoint.Y / svg.Height);
                    foreach (var gradientStop in linearGradientPathFill.Stops)
                    {
                        var stop = new GradientStop
                        {
                            Offset = gradientStop.Offset,
                            Color = ConvertToColor(gradientStop.Color, gradientStop.Opacity)
                        };
                        linearGradientBrush.GradientStops.Add(stop);
                    }
                    var transformGroup = new TransformGroup();
                    foreach (var transform in linearGradientPathFill.Transforms)
                    {
                        if (transform.StartsWith("translate"))
                        {

                        }
                        else if (transform.StartsWith("rotate"))
                        {
                            
                        }
                        else if (transform.StartsWith("scale"))
                        {
                            
                        }
                    }
                    linearGradientBrush.Transform = transformGroup;
                    break;

                case RadialGradientPathFill radialGradientPathFill:
                    brush = new RadialGradientBrush();
                    var radialGradientBrush = brush as RadialGradientBrush;
                    radialGradientBrush!.Center = new Point(radialGradientPathFill.Center.X / svg.Width, radialGradientPathFill.Center.Y / svg.Height);
                    radialGradientBrush.RadiusX = radialGradientPathFill.Radius / svg.Width;
                    radialGradientBrush.RadiusY = radialGradientPathFill.Radius / svg.Height;
                    foreach (var gradientStop in radialGradientPathFill.Stops)
                    {
                        var stop = new GradientStop
                        {
                            Offset = gradientStop.Offset,
                            Color = ConvertToColor(gradientStop.Color, gradientStop.Opacity)
                        };
                        radialGradientBrush.GradientStops.Add(stop);
                    }
                    break;

                default:
                    brush = new SolidColorBrush(Colors.Black);
                    break;
            }
            grid.Children.Add(new Path()
            {
                Data = data,
                Fill = brush
            });
        }

        return grid;
    }

    private Geometry PathMarkupToGeometry(string pathMarkup)
    {
        string xaml =
            "<Path " +
            "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
            "<Path.Data>" + pathMarkup + "</Path.Data></Path>";

        Path path = XamlReader.Load(xaml) as Path;

        // Detach the PathGeometry from the Path
        Geometry geometry = path.Data;
        path.Data = null;
        return geometry;
    }

    private Color ConvertToColor(string hex, double opacity)
    {
        byte alpha = (byte)(opacity * 255);

        byte r = Convert.ToByte(hex.Substring(1, 2), 16);
        byte g = Convert.ToByte(hex.Substring(3, 2), 16);
        byte b = Convert.ToByte(hex.Substring(5, 2), 16);

        return Color.FromArgb(alpha, r, g, b);
    }
}