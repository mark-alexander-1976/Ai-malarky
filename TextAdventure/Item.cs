namespace TextAdventure;

public class Item
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsFixed { get; init; }  // Cannot be picked up (e.g., a locked door)

    public override string ToString() => Name;
}
