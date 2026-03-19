namespace TextAdventure.Models;

public class Room
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public List<Exit> Exits { get; init; } = [];
    public bool IsDark { get; init; }
}
