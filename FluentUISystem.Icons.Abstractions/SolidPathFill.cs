namespace FluentUISystem.Icons.Abstractions
{
    public class SolidPathFill : IPathFill
    {
        public SolidPathFill()
        {
        }

        public SolidPathFill(string color)
        {
            Color = color;
        }

        public string FillId => "SolidPathFill";

        public PathFillType Type => PathFillType.Solid;

        public string Color { get; set; } = string.Empty;
    }
}
