static class WorldFactory
{
    public static World CreateWorld()
    {
        var items = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["lamp"] = new ItemDefinition
            {
                Id = "lamp",
                Name = "Brass Lamp",
                ShortName = "LAMP",
                Description = "A battered brass lamp. It still holds enough oil for a long night underground. USE LAMP will light or extinguish it."
            },
            ["rope"] = new ItemDefinition
            {
                Id = "rope",
                Name = "Coiled Rope",
                ShortName = "ROPE",
                Description = "A stout hemp rope with an iron hook tied to one end."
            },
            ["flute"] = new ItemDefinition
            {
                Id = "flute",
                Name = "Reed Flute",
                ShortName = "FLUTE",
                Description = "A simple reed flute. Its notes are thin, eerie, and carry a long way in stone halls."
            },
            ["brass-key"] = new ItemDefinition
            {
                Id = "brass-key",
                Name = "Brass Key",
                ShortName = "KEY",
                Description = "A square-cut brass key, green with age. It looks as if it belongs to an old grate lock."
            },
            ["silver-key"] = new ItemDefinition
            {
                Id = "silver-key",
                Name = "Silver Key",
                ShortName = "SILVER KEY",
                Description = "A bright silver key worked in the shape of a crescent moon."
            },
            ["opal-seal"] = new ItemDefinition
            {
                Id = "opal-seal",
                Name = "Opal Seal",
                ShortName = "SEAL",
                Description = "A thumb-sized seal carved from cloudy opal. Cold light trembles inside it.",
                IsTreasure = true
            },
            ["silver-chalice"] = new ItemDefinition
            {
                Id = "silver-chalice",
                Name = "Silver Chalice",
                ShortName = "CHALICE",
                Description = "A silver cup blackened by age, still set with tiny river pearls.",
                IsTreasure = true
            },
            ["gold-idol"] = new ItemDefinition
            {
                Id = "gold-idol",
                Name = "Gold Idol",
                ShortName = "IDOL",
                Description = "A squat idol of hammered gold, warm to the touch despite the chill air.",
                IsTreasure = true
            },
            ["jeweled-crown"] = new ItemDefinition
            {
                Id = "jeweled-crown",
                Name = "Jeweled Crown",
                ShortName = "CROWN",
                Description = "A narrow crown of tarnished electrum set with dark red stones.",
                IsTreasure = true
            },
            ["ruby-scarab"] = new ItemDefinition
            {
                Id = "ruby-scarab",
                Name = "Ruby Scarab",
                ShortName = "SCARAB",
                Description = "A scarab carved from deep ruby. It flashes like a coal when the light catches it.",
                IsTreasure = true
            }
        };

        var rooms = new Dictionary<string, Room>(StringComparer.OrdinalIgnoreCase)
        {
            ["village-green"] = new Room
            {
                Id = "village-green",
                Name = "Village Green",
                Description = "You are standing on the village green beside a worn stone plinth. Old stories say lost treasures laid here are counted as safely home."
            },
            ["north-road"] = new Room
            {
                Id = "north-road",
                Name = "North Road",
                Description = "A cart-rutted road runs between thorn hedges. The village lies south while older country gathers ahead."
            },
            ["east-road"] = new Room
            {
                Id = "east-road",
                Name = "East Road",
                Description = "The road bends past wind-bent elms. Wheel marks lead east toward ruined stonework."
            },
            ["south-road"] = new Room
            {
                Id = "south-road",
                Name = "South Road",
                Description = "The village sounds fade here. Broken paving descends toward colder ground to the south."
            },
            ["west-road"] = new Room
            {
                Id = "west-road",
                Name = "West Road",
                Description = "The western lane narrows beneath leaning trees and a restless breeze."
            },
            ["lantern-hut"] = new Room
            {
                Id = "lantern-hut",
                Name = "Lantern Hut",
                Description = "This abandoned watchman's hut smells of oil and dust. Shelves sag against the walls.",
                ItemIds = { "lamp" }
            },
            ["stone-bridge"] = new Room
            {
                Id = "stone-bridge",
                Name = "Stone Bridge",
                Description = "A cracked stone bridge crosses a narrow ravine. Beyond it rises the dark shape of a scholar's tower."
            },
            ["old-well"] = new Room
            {
                Id = "old-well",
                Name = "Old Well",
                Description = "An ancient well of dressed stone stands here. Iron bars guard the shaft mouth and a heavy lock hangs from the grate."
            },
            ["mossy-clearing"] = new Room
            {
                Id = "mossy-clearing",
                Name = "Mossy Clearing",
                Description = "A quiet clearing glows green in the shade. The ground is springy with thick moss.",
                ItemIds = { "rope" }
            },
            ["watchtower-base"] = new Room
            {
                Id = "watchtower-base",
                Name = "Watchtower Base",
                Description = "A round tower rises overhead. Its lower room is bare except for wind and dust.",
                ItemIds = { "brass-key" }
            },
            ["whispering-glen"] = new Room
            {
                Id = "whispering-glen",
                Name = "Whispering Glen",
                Description = "A narrow glen folds away from the lane. Reeds hiss in a hidden stream.",
                ItemIds = { "flute" }
            },
            ["sunken-stair"] = new Room
            {
                Id = "sunken-stair",
                Name = "Sunken Stair",
                Description = "Half-buried steps descend between cracked stones. A rusted iron door below leads into blackness."
            },
            ["crypt-entry"] = new Room
            {
                Id = "crypt-entry",
                Name = "Crypt Entry",
                Description = "Cold stone arches frame a buried crossing. Water drips somewhere in the dark.",
                IsDark = true,
                DarkDescription = "It is pitch dark in the crypt entry. Without a lamp the darkness feels solid enough to touch."
            },
            ["echo-chamber"] = new Room
            {
                Id = "echo-chamber",
                Name = "Echo Chamber",
                Description = "A wide chamber of smooth stone throws every sound back at you. Something valuable glints on a ledge.",
                ItemIds = { "opal-seal" }
            },
            ["underground-lake"] = new Room
            {
                Id = "underground-lake",
                Name = "Underground Lake",
                Description = "Black water laps at a shelf of rock. A pale treasure sits in a dry niche above the shore.",
                IsDark = true,
                DarkDescription = "You hear slow water in the dark and smell the sharp scent of wet stone.",
                ItemIds = { "silver-chalice" }
            },
            ["rope-chasm"] = new Room
            {
                Id = "rope-chasm",
                Name = "Rope Chasm",
                Description = "A split in the earth opens beneath a natural arch. Someone once set iron rings into the rock here.",
                IsDark = true,
                DarkDescription = "A hollow draft moves through total darkness. Somewhere nearby the ground falls away."
            },
            ["serpent-shrine"] = new Room
            {
                Id = "serpent-shrine",
                Name = "Serpent Shrine",
                Description = "A shrine of green stone crouches here. A great carved serpent coils around the central altar.",
                ItemIds = { "gold-idol" }
            },
            ["wizard-study"] = new Room
            {
                Id = "wizard-study",
                Name = "Wizard Study",
                Description = "Dusty books and broken instruments lie everywhere. Moon symbols are etched on the floor.",
                ItemIds = { "silver-key" }
            },
            ["moon-tower"] = new Room
            {
                Id = "moon-tower",
                Name = "Moon Tower",
                Description = "The tower top is open to the sky. A stone chair faces the moon and a royal prize rests upon it.",
                ItemIds = { "jeweled-crown" }
            },
            ["treasure-vault"] = new Room
            {
                Id = "treasure-vault",
                Name = "Treasure Vault",
                Description = "A low vault lined with metal-bound chests. One pedestal stands open and empty save for a ruby scarab.",
                ItemIds = { "ruby-scarab" },
                IsDark = true,
                DarkDescription = "The air is close and stale. Metal and stone surround you in absolute darkness."
            }
        };

        Connect(rooms, "village-green", Direction.North, "north-road");
        Connect(rooms, "village-green", Direction.East, "east-road");
        Connect(rooms, "village-green", Direction.South, "south-road");
        Connect(rooms, "village-green", Direction.West, "west-road");
        Connect(rooms, "north-road", Direction.North, "watchtower-base");
        Connect(rooms, "north-road", Direction.East, "old-well");
        Connect(rooms, "east-road", Direction.North, "lantern-hut");
        Connect(rooms, "east-road", Direction.East, "stone-bridge");
        Connect(rooms, "west-road", Direction.North, "whispering-glen");
        Connect(rooms, "west-road", Direction.West, "mossy-clearing");
        Connect(rooms, "south-road", Direction.South, "sunken-stair");
        Connect(rooms, "watchtower-base", Direction.Up, "moon-tower");
        Connect(rooms, "stone-bridge", Direction.North, "wizard-study");
        Connect(rooms, "whispering-glen", Direction.North, "echo-chamber");
        Connect(rooms, "sunken-stair", Direction.Down, "underground-lake");
        Connect(rooms, "old-well", Direction.Down, "crypt-entry");
        Connect(rooms, "crypt-entry", Direction.South, "underground-lake");
        Connect(rooms, "crypt-entry", Direction.East, "echo-chamber");
        Connect(rooms, "echo-chamber", Direction.East, "serpent-shrine");
        Connect(rooms, "underground-lake", Direction.East, "rope-chasm");
        Connect(rooms, "mossy-clearing", Direction.South, "rope-chasm");
        Connect(rooms, "wizard-study", Direction.East, "moon-tower");
        Connect(rooms, "moon-tower", Direction.South, "treasure-vault");
        Connect(rooms, "rope-chasm", Direction.East, "treasure-vault");

        return new World
        {
            StartRoomId = "village-green",
            Rooms = rooms,
            Items = items,
            TreasureIds = new HashSet<string>(new[]
            {
                "opal-seal",
                "silver-chalice",
                "gold-idol",
                "jeweled-crown",
                "ruby-scarab"
            }, StringComparer.OrdinalIgnoreCase)
        };
    }

    private static void Connect(Dictionary<string, Room> rooms, string from, Direction direction, string to)
    {
        rooms[from].Exits[direction] = to;
        rooms[to].Exits[Opposite(direction)] = from;
    }

    private static Direction Opposite(Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}