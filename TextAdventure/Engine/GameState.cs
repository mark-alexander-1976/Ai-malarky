using TextAdventure.Models;

namespace TextAdventure.Engine;

public class GameState
{
    public const int InventoryLocation = -1;
    public const int NowhereLocation = -2;
    public const int StartRoom = 0;
    public const int TreasureRoom = 0;
    public const int MaxInventory = 6;

    public Dictionary<int, Room> Rooms { get; } = [];
    public List<Item> Items { get; } = [];

    public int CurrentRoomId { get; set; } = StartRoom;
    public int Moves { get; set; }
    public bool LampLit { get; set; }
    public bool DragonAsleep { get; set; }
    public bool GameOver { get; set; }

    public Room CurrentRoom => Rooms[CurrentRoomId];

    public IEnumerable<Item> ItemsInRoom(int roomId) =>
        Items.Where(i => i.RoomId == roomId);

    public IEnumerable<Item> ItemsHere => ItemsInRoom(CurrentRoomId);
    public IEnumerable<Item> PlayerItems => ItemsInRoom(InventoryLocation);

    public int TreasureCount => Items.Count(i => i.IsTreasure);
    public int TreasuresScored => Items.Count(i => i.IsTreasure && i.RoomId == TreasureRoom);

    public bool HasItem(string name) =>
        Items.Any(i => i.RoomId == InventoryLocation && i.Matches(name));

    public bool IsHere(string name) =>
        Items.Any(i => i.RoomId == CurrentRoomId && i.Matches(name));

    public Item? FindItem(string name, int location) =>
        Items.FirstOrDefault(i => i.RoomId == location && i.Matches(name));

    public Item? FindItemAnywhere(string name) =>
        FindItem(name, InventoryLocation) ?? FindItem(name, CurrentRoomId);

    public bool CanSee =>
        !CurrentRoom.IsDark || (LampLit && HasItem("lamp"));
}
