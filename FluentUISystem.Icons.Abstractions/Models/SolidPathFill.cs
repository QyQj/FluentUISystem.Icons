namespace FluentUISystem.Icons.Abstractions.Models
{
    public class SolidPathFill : PathFillDefinition
    {
        public SolidPathFill()
        {
            
        }

        public SolidPathFill(string id)
        {
            FillId = id;
        }
        public string Color { get; set; } = string.Empty;
    }
}