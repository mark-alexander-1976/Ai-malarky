using TextAdventure.Models;

namespace TextAdventure.Engine;

public class GameState
{
    public const int InventoryLocation = -1;
    public const int NowhereLocation = -2;
    public const int StartRoom = 0;
    public const int TreasureRoom = 0;
    public const int MaxInventory = 6;
    public const int TotalSquirrels = 5;

    private readonly Random _rng = new();

    public static readonly (string Name, string Description, string DefeatMessage)[] Squirrels =
    [
        ("Nutkin the Nimble",
         "A fast grey squirrel darts in front of you, baring tiny fangs!",
         "You lunge at Nutkin! The nimble squirrel dodges twice but you finally grab it by the tail. It squeaks and scurries away!"),
        ("Acorn the Aggressive",
         "A furious red squirrel charges at you, pelting acorns like tiny missiles!",
         "You swat away the barrage of acorns and give Acorn a firm boot. It tumbles away, chittering angrily!"),
        ("Shadow the Sneaky",
         "A pitch-black squirrel drops from above and lands on your head!",
         "You peel Shadow off your head and toss it into the bushes. It vanishes into the darkness with a hiss!"),
        ("Bushy the Brave",
         "An enormous squirrel with a magnificently bushy tail blocks your path, standing on its hind legs like a tiny warrior!",
         "You stare Bushy down. After a tense standoff, you feint left and dash right. Bushy tumbles aside, defeated!"),
        ("Chester the Cheeky",
         "A tiny squirrel appears and immediately bites your ankle! It looks up at you with mischievous eyes.",
         "You shake Chester off your ankle and flick the cheeky critter away. It bounces off a tree and disappears, squeaking indignantly!")
    ];

    public Dictionary<int, Room> Rooms { get; } = [];
    public List<Item> Items { get; } = [];

    public int CurrentRoomId { get; set; } = StartRoom;
    public int Moves { get; set; }
    public bool LampLit { get; set; }
    public bool DragonAsleep { get; set; }
    public bool GameOver { get; set; }

    // Squirrel encounter state
    public int SquirrelsDefeated { get; set; }
    public bool InSquirrelEncounter { get; set; }
    public string? CurrentSquirrelName { get; set; }
    public bool AllSquirrelsDefeated => SquirrelsDefeated >= TotalSquirrels;

    public bool ShouldEncounterSquirrel() =>
        !AllSquirrelsDefeated && _rng.Next(100) < 30;

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
