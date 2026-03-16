namespace FluentUISystem.Icons.Shared
{
    public class SolidPathFill : IPathFill
    {
        public string Id => "SolidPathFill";

        public PathFillType Type => PathFillType.Solid;

        public double Opacity { get; set; }

        public string Color { get; set; }
    }
}