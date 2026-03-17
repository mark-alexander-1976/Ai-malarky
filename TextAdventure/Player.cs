namespace TextAdventure;

public class Player
{
    public int CurrentLocationId { get; set; } = 1;
    public List<Item> Inventory { get; } = [];
    public int Health { get; set; } = 100;
    public bool IsAlive => Health > 0;
    public bool HasWon { get; set; }
    public bool DragonDefeated { get; set; }
    public bool QuitRequested { get; set; }

    public bool HasItem(string name) =>
        Inventory.Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public Item? GetItem(string name) =>
        Inventory.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public bool HasLightSource() =>
        HasItem("Lantern") || HasItem("Torch");
}
