using System.Text;

enum Direction
{
    North,
    South,
    East,
    West,
    Up,
    Down
}

sealed record ParsedCommand(string Verb, string? Noun);

sealed class ItemDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string ShortName { get; init; } = string.Empty;
    public bool IsTreasure { get; init; }

    public bool Matches(string noun)
    {
        var normalized = Normalize(noun);
        return normalized == Normalize(Name) || (!string.IsNullOrWhiteSpace(ShortName) && normalized == Normalize(ShortName));
    }

    public static string Normalize(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Trim().ToUpperInvariant())
        {
            if (char.IsLetterOrDigit(character) || character == ' ')
            {
                builder.Append(character);
            }
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

sealed class Room
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string DarkDescription { get; init; } = "It is pitch dark. You can feel damp stone and stale air around you.";
    public bool IsDark { get; init; }
    public Dictionary<Direction, string> Exits { get; } = new();
    public List<string> ItemIds { get; } = new();
}

sealed class World
{
    public required string StartRoomId { get; init; }
    public required Dictionary<string, Room> Rooms { get; init; }
    public required Dictionary<string, ItemDefinition> Items { get; init; }
    public required HashSet<string> TreasureIds { get; init; }

    public void Validate()
    {
        if (Rooms.Count != 20)
        {
            throw new InvalidOperationException($"Expected 20 rooms, found {Rooms.Count}.");
        }

        if (!Rooms.ContainsKey(StartRoomId))
        {
            throw new InvalidOperationException("The start room is missing.");
        }

        foreach (var room in Rooms.Values)
        {
            foreach (var exit in room.Exits)
            {
                if (!Rooms.ContainsKey(exit.Value))
                {
                    throw new InvalidOperationException($"Room '{room.Id}' has an exit to unknown room '{exit.Value}'.");
                }
            }

            foreach (var itemId in room.ItemIds)
            {
                if (!Items.ContainsKey(itemId))
                {
                    throw new InvalidOperationException($"Room '{room.Id}' contains unknown item '{itemId}'.");
                }
            }
        }

        foreach (var treasureId in TreasureIds)
        {
            if (!Items.TryGetValue(treasureId, out var treasure) || !treasure.IsTreasure)
            {
                throw new InvalidOperationException($"Treasure '{treasureId}' is not defined as a treasure.");
            }
        }
    }
}

sealed class GameState
{
    public required World World { get; init; }
    public required string CurrentRoomId { get; set; }
    public HashSet<string> Inventory { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Flags { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> SecuredTreasures { get; } = new(StringComparer.OrdinalIgnoreCase);
    public bool AutoMiniMapEnabled { get; set; }
    public int SquirrelsDefeated { get; set; }
    public int RequiredSquirrelTrials { get; init; } = 5;
    public int? ActiveSquirrelLevel { get; set; }
    public int Moves { get; set; }
    public bool QuitRequested { get; set; }
    public bool Won { get; set; }
    public Room CurrentRoom => World.Rooms[CurrentRoomId];
}

sealed class SaveData
{
    public required string CurrentRoomId { get; init; }
    public required List<string> Inventory { get; init; }
    public required List<string> Flags { get; init; }
    public required List<string> SecuredTreasures { get; init; }
    public required Dictionary<string, List<string>> RoomItems { get; init; }
    public bool AutoMiniMapEnabled { get; init; }
    public int SquirrelsDefeated { get; init; }
    public int? ActiveSquirrelLevel { get; init; }
    public int Moves { get; init; }
}