namespace TextAdventure;

public class Location
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    /// <summary>Exits keyed by direction word, value is destination location ID.</summary>
    public Dictionary<string, int> Exits { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Exits that require the player to carry a specific item (direction -> item name).</summary>
    public Dictionary<string, string> LockedExits { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Items currently in this location.</summary>
    public List<Item> Items { get; init; } = [];

    /// <summary>When true the location is pitch dark without a light source.</summary>
    public bool IsDark { get; init; }

    /// <summary>When true entering without protection causes damage.</summary>
    public bool IsDangerous { get; init; }

    /// <summary>Item the player must carry to avoid danger (null = always dangerous).</summary>
    public string? SafeItemName { get; init; }

    public bool HasBeenVisited { get; set; }
}
