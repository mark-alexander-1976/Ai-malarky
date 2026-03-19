namespace TextAdventure.Models;

public class Item
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public int RoomId { get; set; }          // -1 = player inventory, -2 = nowhere/consumed
    public bool IsTreasure { get; init; }
    public string[]? Aliases { get; init; }

    public bool Matches(string name) =>
        Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
        (Aliases?.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? false);
}
