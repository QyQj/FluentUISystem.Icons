namespace FluentUISystem.Icons.Shared
{
    public interface IPathFill
    {
        string Id { get; }
        
        PathFillType Type { get; }

        double Opacity { get; set; }
    }
}