using TextAdventure.Models;

namespace TextAdventure.Engine;

public static class WorldBuilder
{
    public static void Build(GameState state)
    {
        BuildRooms(state);
        BuildExits(state);
        BuildItems(state);
    }

    private static void BuildRooms(GameState state)
    {
        var rooms = new Room[]
        {
            new() { Id = 0,  Name = "Sunny Meadow",
                Description = "You are in a bright, sunny meadow. Wildflowers sway gently.\nA wooden sign reads: \"BRING TREASURES HERE FOR GLORY!\"\nPaths lead NORTH, EAST and SOUTH." },
            new() { Id = 1,  Name = "Dark Forest",
                Description = "Tall trees block the sunlight. Strange sounds echo around you.\nPaths lead SOUTH, EAST and NORTH." },
            new() { Id = 2,  Name = "Old Oak Tree",
                Description = "An enormous, ancient oak towers above you. A hollow in the trunk\nglows faintly. Paths lead WEST and NORTH." },
            new() { Id = 3,  Name = "Murky Swamp",
                Description = "You stand at the edge of a bubbling swamp. The air stinks.\nStepping stones lead EAST. A path goes SOUTH." },
            new() { Id = 4,  Name = "Ancient Ruins",
                Description = "Crumbling stone walls hint at a lost civilisation.\nA staircase descends DOWN. Paths lead WEST and SOUTH." },
            new() { Id = 5,  Name = "Underground Chamber", IsDark = true,
                Description = "A vast underground chamber. Stalactites drip overhead.\nTunnels lead UP, NORTH and EAST." },
            new() { Id = 6,  Name = "Crystal Cavern", IsDark = true,
                Description = "The walls glitter with thousands of crystals, casting rainbow light\nwhen illuminated. A tunnel leads WEST." },
            new() { Id = 7,  Name = "Rickety Bridge",
                Description = "A rope bridge spans a deep chasm. It creaks ominously.\nThe far side leads NORTH. Behind you is SOUTH." },
            new() { Id = 8,  Name = "Mountain Ledge",
                Description = "A narrow ledge on a craggy mountain. Wind howls around you.\nA cave entrance leads EAST. The bridge is to the SOUTH." },
            new() { Id = 9,  Name = "Dragon's Lair",
                Description = "A huge cavern, scorched black. Bones litter the floor.\nThe only exit is WEST." },
            new() { Id = 10, Name = "Hidden Garden",
                Description = "A walled garden blooming with magical flowers.\nA rusty gate leads SOUTH. A gap in the wall goes WEST." },
            new() { Id = 11, Name = "Locked Vault",
                Description = "A small stone vault. Shelves line the walls.\nThe only exit is NORTH." },
            new() { Id = 12, Name = "Vine-Covered Passage",
                Description = "Thick vines choke this narrow passage.\nOpenings lead NORTH and SOUTH." },
        };

        foreach (var room in rooms)
            state.Rooms[room.Id] = room;
    }

    private static void BuildExits(GameState state)
    {
        state.Rooms[0].Exits.AddRange([
            new("north", 1),
            new("east",  12, "axe", "Thick vines block the way east. If only you could CUT them..."),
            new("south", 7)
        ]);
        state.Rooms[1].Exits.AddRange([
            new("south", 0),
            new("east",  2),
            new("north", 3)
        ]);
        state.Rooms[2].Exits.AddRange([
            new("west",  1),
            new("north", 4)
        ]);
        state.Rooms[3].Exits.AddRange([
            new("east",  4),
            new("south", 1)
        ]);
        state.Rooms[4].Exits.AddRange([
            new("west",  3),
            new("south", 2),
            new("down",  5)
        ]);
        state.Rooms[5].Exits.AddRange([
            new("up",    4),
            new("north", 6),
            new("east",  11, "key", "A heavy iron door blocks the way. There is a keyhole.")
        ]);
        state.Rooms[6].Exits.AddRange([
            new("west",  5)
        ]);
        state.Rooms[7].Exits.AddRange([
            new("north", 0),
            new("south", 8, "rope", "The bridge has a huge gap! You'll need something to cross it.")
        ]);
        state.Rooms[8].Exits.AddRange([
            new("south", 7),
            new("east",  9)
        ]);
        state.Rooms[9].Exits.AddRange([
            new("west",  8)
        ]);
        state.Rooms[10].Exits.AddRange([
            new("south", 1),
            new("west",  3)
        ]);
        state.Rooms[11].Exits.AddRange([
            new("north", 5)
        ]);
        state.Rooms[12].Exits.AddRange([
            new("north", 10),
            new("south", 0)
        ]);
    }

    private static void BuildItems(GameState state)
    {
        state.Items.AddRange([
            new() { Name = "axe",       Description = "A sharp woodsman's axe.",                     RoomId = 1 },
            new() { Name = "lamp",      Description = "A battered brass oil lamp.",                  RoomId = 2 },
            new() { Name = "matches",   Description = "A small box of dry matches.",                 RoomId = 3 },
            new() { Name = "key",       Description = "A heavy iron key.",                           RoomId = 10 },
            new() { Name = "rope",      Description = "A coil of sturdy rope.",                      RoomId = 4 },
            new() { Name = "repellent", Description = "A bottle labelled \"DRAGON-B-GONE\".",        RoomId = 12,
                    Aliases = ["bottle", "dragon-b-gone"] },
            new() { Name = "bread",     Description = "A loaf of stale bread.",                      RoomId = 3 },
            // Treasures
            new() { Name = "crown",     Description = "*Golden Crown* - it gleams magnificently!",   RoomId = 9,  IsTreasure = true,
                    Aliases = ["golden"] },
            new() { Name = "diamond",   Description = "*Diamond Ring* - light dances within!",       RoomId = 6,  IsTreasure = true,
                    Aliases = ["ring"] },
            new() { Name = "chalice",   Description = "*Ruby Chalice* - dark red and beautiful!",    RoomId = 11, IsTreasure = true,
                    Aliases = ["ruby"] },
            new() { Name = "wand",      Description = "*Magic Wand* - it tingles in your hand!",     RoomId = 5,  IsTreasure = true,
                    Aliases = ["magic"] },
            new() { Name = "scroll",    Description = "*Ancient Scroll* - arcane runes shimmer!",    RoomId = 8,  IsTreasure = true,
                    Aliases = ["ancient"] },
        ]);
    }
}
