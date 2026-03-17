using System;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace FluentUISystem.Icons.WinUI3;

internal static class Utils
{
    internal static Geometry PathMarkupToGeometry(string pathMarkup)
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

    internal static Color ConvertToColor(string hex, double opacity)
    {
        byte alpha = (byte)(opacity * 255);

        byte r = Convert.ToByte(hex.Substring(1, 2), 16);
        byte g = Convert.ToByte(hex.Substring(3, 2), 16);
        byte b = Convert.ToByte(hex.Substring(5, 2), 16);

        return Color.FromArgb(alpha, r, g, b);
    }
}