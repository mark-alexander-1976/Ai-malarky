using System.Text.Json;

sealed class Game
{
    private const string SaveFileName = "adventureland-save.json";
    private static readonly JsonSerializerOptions SaveOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private GameState state;

    public Game()
    {
        state = CreateNewState();
    }

    public void Run()
    {
        ShowBanner();
        DescribeCurrentRoom(forceFullDescription: true);

        while (!state.QuitRequested && !state.Won)
        {
            Console.Write("\n> ");
            var input = Console.ReadLine();

            if (input is null)
            {
                Console.WriteLine("The world fades before your eyes.");
                return;
            }

            Execute(Parse(input));
        }

        if (state.Won)
        {
            Console.WriteLine("You have finished the adventure. The village bells ring your name.");
        }
    }

    private static GameState CreateNewState()
    {
        var world = WorldFactory.CreateWorld();
        world.Validate();

        return new GameState
        {
            World = world,
            CurrentRoomId = world.StartRoomId
        };
    }

    private void Execute(ParsedCommand command)
    {
        switch (command.Verb)
        {
            case "":
                Console.WriteLine("Speak up.");
                return;
            case "LOOK":
                DescribeCurrentRoom(forceFullDescription: true);
                return;
            case "HELP":
                ShowHelp();
                return;
            case "INVENTORY":
                ShowInventory();
                return;
            case "SCORE":
                ShowScore();
                return;
            case "SAVE":
                SaveGame();
                return;
            case "LOAD":
                LoadGame();
                return;
            case "QUIT":
                state.QuitRequested = true;
                Console.WriteLine("The adventure ends here.");
                return;
            case "GO":
                HandleGo(command.Noun);
                return;
            case "GET":
                HandleGet(command.Noun);
                return;
            case "DROP":
                HandleDrop(command.Noun);
                return;
            case "EXAMINE":
                HandleExamine(command.Noun);
                return;
            case "USE":
                HandleUse(command.Noun);
                return;
            case "READ":
                HandleRead(command.Noun);
                return;
            case "OPEN":
                HandleOpen(command.Noun);
                return;
            case "CLIMB":
                HandleClimb(command.Noun);
                return;
            default:
                Console.WriteLine("I do not understand that command.");
                return;
        }
    }

    private void HandleGo(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("GO where?");
            return;
        }

        if (!TryParseDirection(noun, out var direction))
        {
            Console.WriteLine("You can go NORTH, SOUTH, EAST, WEST, UP, or DOWN.");
            return;
        }

        Move(direction);
    }

    private void Move(Direction direction)
    {
        var room = state.CurrentRoom;
        if (!room.Exits.TryGetValue(direction, out var targetRoomId))
        {
            Console.WriteLine("You cannot go that way.");
            return;
        }

        var blockedMessage = GetBlockedMovementMessage(room.Id, direction);
        if (blockedMessage is not null)
        {
            Console.WriteLine(blockedMessage);
            return;
        }

        state.CurrentRoomId = targetRoomId;
        state.Moves++;
        DescribeCurrentRoom(forceFullDescription: false);
    }

    private string? GetBlockedMovementMessage(string roomId, Direction direction)
    {
        if ((roomId == "old-well" || roomId == "sunken-stair") && direction == Direction.Down && !state.Flags.Contains("grate-unlocked"))
        {
            return "A locked iron grate bars the way down.";
        }

        if (roomId == "underground-lake" && direction == Direction.East && !state.Flags.Contains("rope-secured"))
        {
            return "The chasm edge is too dangerous. A rope might make the crossing possible.";
        }

        if ((roomId == "moon-tower" && direction == Direction.South) || (roomId == "rope-chasm" && direction == Direction.East))
        {
            if (!state.Flags.Contains("vault-unlocked"))
            {
                return "A moon-marked door is locked fast.";
            }
        }

        return null;
    }

    private void HandleGet(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("GET what?");
            return;
        }

        if (state.CurrentRoom.IsDark && !state.Flags.Contains("lamp-lit"))
        {
            Console.WriteLine("It is too dark to see anything to take.");
            return;
        }

        var itemId = FindVisibleRoomItem(noun);
        if (itemId is null)
        {
            Console.WriteLine("There is no such thing here.");
            return;
        }

        if (itemId == "gold-idol" && !state.Flags.Contains("serpent-charmed"))
        {
            Console.WriteLine("The carved serpent seems almost alive. You cannot bring yourself to snatch the idol while it looms over you.");
            return;
        }

        state.CurrentRoom.ItemIds.Remove(itemId);
        state.Inventory.Add(itemId);
        state.Moves++;
        Console.WriteLine($"Taken: {state.World.Items[itemId].Name}.");
    }

    private void HandleDrop(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("DROP what?");
            return;
        }

        var itemId = FindInventoryItem(noun);
        if (itemId is null)
        {
            Console.WriteLine("You are not carrying that.");
            return;
        }

        state.Inventory.Remove(itemId);
        state.Moves++;

        if (state.CurrentRoomId == "village-green" && state.World.TreasureIds.Contains(itemId))
        {
            state.SecuredTreasures.Add(itemId);
            Console.WriteLine($"You place the {state.World.Items[itemId].Name} on the stone plinth. It is now safe.");

            if (state.SecuredTreasures.Count == state.World.TreasureIds.Count)
            {
                state.Won = true;
                Console.WriteLine("As the last treasure touches the plinth, a bell begins to toll across the valley.");
            }

            return;
        }

        state.CurrentRoom.ItemIds.Add(itemId);
        Console.WriteLine($"Dropped: {state.World.Items[itemId].Name}.");
    }

    private void HandleExamine(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("EXAMINE what?");
            return;
        }

        var itemId = FindInventoryItem(noun) ?? FindVisibleRoomItem(noun);
        if (itemId is not null)
        {
            Console.WriteLine(state.World.Items[itemId].Description);
            return;
        }

        if (ItemDefinition.Normalize(noun) is "ROOM" or "HERE")
        {
            DescribeCurrentRoom(forceFullDescription: true);
            return;
        }

        Console.WriteLine("You see nothing special.");
    }

    private void HandleUse(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("USE what?");
            return;
        }

        var itemId = FindInventoryItem(noun);
        if (itemId is null)
        {
            Console.WriteLine("You are not carrying that.");
            return;
        }

        switch (itemId)
        {
            case "lamp":
                ToggleLamp();
                return;
            case "brass-key":
                UseBrassKey();
                return;
            case "rope":
                UseRope();
                return;
            case "flute":
                UseFlute();
                return;
            case "silver-key":
                UseSilverKey();
                return;
            default:
                Console.WriteLine("Nothing happens.");
                return;
        }
    }

    private void HandleRead(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("READ what?");
            return;
        }

        var target = ItemDefinition.Normalize(noun);

        if (state.CurrentRoomId == "village-green" && target is "PLINTH" or "STONE" or "INSCRIPTION")
        {
            Console.WriteLine("The worn letters read: RETURN THE FIVE LOST TREASURES HERE AND THE LAND SHALL REST AGAIN.");
            return;
        }

        if (state.CurrentRoomId == "wizard-study" && target is "BOOK" or "BOOKS" or "SCROLL" or "NOTE")
        {
            Console.WriteLine("A brittle note reads: THE SILVER KEY TURNS BOTH THE MOON DOOR ABOVE AND ITS TWIN BELOW THE CHASM.");
            return;
        }

        if (state.CurrentRoomId == "treasure-vault" && target is "CHEST" or "CHESTS")
        {
            Console.WriteLine("Dust and old account marks cover the chest lids, but none yield any further secret.");
            return;
        }

        var itemId = FindInventoryItem(noun) ?? FindVisibleRoomItem(noun);
        if (itemId is not null)
        {
            Console.WriteLine("There is nothing written on it worth noting.");
            return;
        }

        Console.WriteLine("There is nothing written there.");
    }

    private void HandleOpen(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("OPEN what?");
            return;
        }

        var target = ItemDefinition.Normalize(noun);

        if (target is "GRATE" or "GATE" or "DOOR")
        {
            if (state.CurrentRoomId is "old-well" or "sunken-stair")
            {
                if (state.Inventory.Contains("brass-key"))
                {
                    UseBrassKey();
                }
                else
                {
                    Console.WriteLine("The lock will not yield without a key.");
                }

                return;
            }
        }

        if (target is "VAULT" or "VAULT DOOR" or "MOON DOOR" or "DOOR")
        {
            if (state.CurrentRoomId is "moon-tower" or "rope-chasm")
            {
                if (state.Inventory.Contains("silver-key"))
                {
                    UseSilverKey();
                }
                else
                {
                    Console.WriteLine("The moon-marked lock requires a proper key.");
                }

                return;
            }
        }

        if (state.CurrentRoomId == "treasure-vault" && target is "CHEST" or "CHESTS")
        {
            Console.WriteLine("The chests are swollen shut and refuse to open.");
            return;
        }

        Console.WriteLine("You cannot open that.");
    }

    private void HandleClimb(string? noun)
    {
        var target = string.IsNullOrWhiteSpace(noun) ? string.Empty : ItemDefinition.Normalize(noun);

        if (state.CurrentRoomId == "watchtower-base" && target is "" or "TOWER" or "STAIR" or "STAIRS")
        {
            Move(Direction.Up);
            return;
        }

        if (state.CurrentRoom.Exits.ContainsKey(Direction.Up) && target is "" or "STAIR" or "STAIRS" or "LADDER" or "TOWER")
        {
            Move(Direction.Up);
            return;
        }

        Console.WriteLine("There is nothing here to climb.");
    }

    private void ToggleLamp()
    {
        if (state.Flags.Contains("lamp-lit"))
        {
            state.Flags.Remove("lamp-lit");
            state.Moves++;
            Console.WriteLine("Your lamp goes out.");
            return;
        }

        state.Flags.Add("lamp-lit");
        state.Moves++;
        Console.WriteLine("Your lamp casts a warm yellow circle of light.");
        if (state.CurrentRoom.IsDark)
        {
            DescribeCurrentRoom(forceFullDescription: true);
        }
    }

    private void UseBrassKey()
    {
        if (state.Flags.Contains("grate-unlocked"))
        {
            Console.WriteLine("The grate is already unlocked.");
            return;
        }

        if (state.CurrentRoomId is "old-well" or "sunken-stair")
        {
            state.Flags.Add("grate-unlocked");
            state.Moves++;
            Console.WriteLine("The brass key turns with a groan. The iron grate unlocks below.");
            return;
        }

        Console.WriteLine("The brass key does not fit anything here.");
    }

    private void UseRope()
    {
        if (state.Flags.Contains("rope-secured"))
        {
            Console.WriteLine("The rope is already secured across the gap.");
            return;
        }

        if (state.CurrentRoomId == "underground-lake")
        {
            state.Flags.Add("rope-secured");
            state.Moves++;
            Console.WriteLine("You hook the rope into the iron ring. It now spans the chasm to the east.");
            return;
        }

        Console.WriteLine("There is nowhere useful to secure the rope here.");
    }

    private void UseFlute()
    {
        if (state.Flags.Contains("serpent-charmed"))
        {
            Console.WriteLine("The shrine is already quiet.");
            return;
        }

        if (state.CurrentRoomId == "serpent-shrine")
        {
            state.Flags.Add("serpent-charmed");
            state.Moves++;
            Console.WriteLine("Your thin tune winds around the shrine. The serpent's menace seems to soften, leaving the idol unguarded.");
            return;
        }

        Console.WriteLine("The notes drift away without effect.");
    }

    private void UseSilverKey()
    {
        if (state.Flags.Contains("vault-unlocked"))
        {
            Console.WriteLine("The vault door is already unlocked.");
            return;
        }

        if (state.CurrentRoomId is "moon-tower" or "rope-chasm")
        {
            state.Flags.Add("vault-unlocked");
            state.Moves++;
            Console.WriteLine("The silver key turns in a moon-marked lock. Somewhere nearby a heavy vault door opens.");
            return;
        }

        Console.WriteLine("The silver key has no lock to answer it here.");
    }

    private void SaveGame()
    {
        var saveData = new SaveData
        {
            CurrentRoomId = state.CurrentRoomId,
            Inventory = state.Inventory.OrderBy(item => item).ToList(),
            Flags = state.Flags.OrderBy(flag => flag).ToList(),
            SecuredTreasures = state.SecuredTreasures.OrderBy(item => item).ToList(),
            Moves = state.Moves,
            RoomItems = state.World.Rooms.Values.ToDictionary(
                room => room.Id,
                room => room.ItemIds.OrderBy(item => item).ToList(),
                StringComparer.OrdinalIgnoreCase)
        };

        File.WriteAllText(GetSaveFilePath(), JsonSerializer.Serialize(saveData, SaveOptions));
        Console.WriteLine($"Game saved to {GetSaveFilePath()}.");
    }

    private void LoadGame()
    {
        var saveFilePath = GetSaveFilePath();
        if (!File.Exists(saveFilePath))
        {
            Console.WriteLine("There is no saved game to load.");
            return;
        }

        SaveData? saveData;

        try
        {
            saveData = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(saveFilePath), SaveOptions);
        }
        catch (Exception)
        {
            Console.WriteLine("The save file is unreadable.");
            return;
        }

        if (saveData is null)
        {
            Console.WriteLine("The save file is empty.");
            return;
        }

        try
        {
            state = RestoreState(saveData);
        }
        catch (InvalidOperationException exception)
        {
            Console.WriteLine($"The save file is invalid: {exception.Message}");
            return;
        }

        Console.WriteLine("Game loaded.");
        DescribeCurrentRoom(forceFullDescription: true);
    }

    private static GameState RestoreState(SaveData saveData)
    {
        var restoredState = CreateNewState();
        var world = restoredState.World;

        if (!world.Rooms.ContainsKey(saveData.CurrentRoomId))
        {
            throw new InvalidOperationException("Saved current room does not exist.");
        }

        restoredState.CurrentRoomId = saveData.CurrentRoomId;
        restoredState.Moves = saveData.Moves;

        restoredState.Inventory.Clear();
        foreach (var itemId in saveData.Inventory)
        {
            ValidateItem(world, itemId, "inventory");
            restoredState.Inventory.Add(itemId);
        }

        restoredState.Flags.Clear();
        foreach (var flag in saveData.Flags)
        {
            restoredState.Flags.Add(flag);
        }

        restoredState.SecuredTreasures.Clear();
        foreach (var itemId in saveData.SecuredTreasures)
        {
            ValidateItem(world, itemId, "secured treasures");
            if (!world.TreasureIds.Contains(itemId))
            {
                throw new InvalidOperationException($"Item '{itemId}' is not a treasure.");
            }

            restoredState.SecuredTreasures.Add(itemId);
        }

        foreach (var room in world.Rooms.Values)
        {
            room.ItemIds.Clear();

            if (!saveData.RoomItems.TryGetValue(room.Id, out var savedItems))
            {
                continue;
            }

            foreach (var itemId in savedItems)
            {
                ValidateItem(world, itemId, $"room '{room.Id}'");
                room.ItemIds.Add(itemId);
            }
        }

        restoredState.Won = false;
        restoredState.QuitRequested = false;
        return restoredState;
    }

    private static void ValidateItem(World world, string itemId, string source)
    {
        if (!world.Items.ContainsKey(itemId))
        {
            throw new InvalidOperationException($"Unknown item '{itemId}' in {source}.");
        }
    }

    private void DescribeCurrentRoom(bool forceFullDescription)
    {
        var room = state.CurrentRoom;
        Console.WriteLine($"\n{room.Name}");

        if (room.IsDark && !state.Flags.Contains("lamp-lit"))
        {
            Console.WriteLine(room.DarkDescription);
            Console.WriteLine($"Exits: {FormatExits(room.Exits.Keys)}");
            return;
        }

        Console.WriteLine(room.Description);
        PrintDynamicRoomText(room);
        PrintVisibleItems(room);
        Console.WriteLine($"Exits: {FormatExits(room.Exits.Keys)}");

        if (forceFullDescription && room.Id == "village-green")
        {
            Console.WriteLine("Your task is to recover the five lost treasures and leave them here on the plinth.");
        }
    }

    private void PrintDynamicRoomText(Room room)
    {
        switch (room.Id)
        {
            case "old-well" when !state.Flags.Contains("grate-unlocked"):
                Console.WriteLine("A locked grate blocks the descent into the shaft.");
                break;
            case "sunken-stair" when !state.Flags.Contains("grate-unlocked"):
                Console.WriteLine("The iron door at the foot of the stair is locked.");
                break;
            case "underground-lake" when !state.Flags.Contains("rope-secured"):
                Console.WriteLine("An iron ring is set into the eastern wall above a treacherous drop.");
                break;
            case "rope-chasm" when !state.Flags.Contains("vault-unlocked"):
                Console.WriteLine("A moon-marked vault door stands to the east.");
                break;
            case "moon-tower" when !state.Flags.Contains("vault-unlocked"):
                Console.WriteLine("A narrow stair curves south toward a locked moon-marked door.");
                break;
            case "serpent-shrine" when !state.Flags.Contains("serpent-charmed"):
                Console.WriteLine("The carved serpent's jeweled eyes seem fixed on the idol.");
                break;
            case "village-green" when state.SecuredTreasures.Count > 0:
                Console.WriteLine($"Secured treasures on the plinth: {state.SecuredTreasures.Count}/{state.World.TreasureIds.Count}.");
                break;
        }
    }

    private void PrintVisibleItems(Room room)
    {
        if (room.ItemIds.Count == 0)
        {
            return;
        }

        var names = room.ItemIds.Select(itemId => state.World.Items[itemId].Name).OrderBy(name => name).ToArray();
        Console.WriteLine($"You can see: {string.Join(", ", names)}.");
    }

    private void ShowInventory()
    {
        if (state.Inventory.Count == 0)
        {
            Console.WriteLine("You are empty-handed.");
            return;
        }

        var names = state.Inventory.Select(itemId => state.World.Items[itemId].Name).OrderBy(name => name).ToArray();
        Console.WriteLine($"You are carrying: {string.Join(", ", names)}.");
    }

    private void ShowScore()
    {
        Console.WriteLine($"You have secured {state.SecuredTreasures.Count} of {state.World.TreasureIds.Count} treasures in {state.Moves} moves.");
    }

    private void ShowHelp()
    {
        Console.WriteLine("Commands: LOOK, GO NORTH, GO SOUTH, GO EAST, GO WEST, GO UP, GO DOWN,");
        Console.WriteLine("GET item, DROP item, EXAMINE item, USE item, READ object, OPEN object,");
        Console.WriteLine("CLIMB, INVENTORY, SCORE, SAVE, LOAD, HELP, QUIT.");
        Console.WriteLine("Short movement commands: N, S, E, W, U, D.");
    }

    private static ParsedCommand Parse(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.Length == 0)
        {
            return new ParsedCommand(string.Empty, null);
        }

        var upper = trimmed.ToUpperInvariant();
        return upper switch
        {
            "N" => new ParsedCommand("GO", "NORTH"),
            "S" => new ParsedCommand("GO", "SOUTH"),
            "E" => new ParsedCommand("GO", "EAST"),
            "W" => new ParsedCommand("GO", "WEST"),
            "U" => new ParsedCommand("GO", "UP"),
            "D" => new ParsedCommand("GO", "DOWN"),
            "L" => new ParsedCommand("LOOK", null),
            "I" => new ParsedCommand("INVENTORY", null),
            _ => ParseWords(trimmed)
        };
    }

    private static ParsedCommand ParseWords(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var verb = parts[0].ToUpperInvariant();
        var noun = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : null;

        return verb switch
        {
            "TAKE" => new ParsedCommand("GET", noun),
            "GRAB" => new ParsedCommand("GET", noun),
            "LIGHT" => new ParsedCommand("USE", noun),
            "UNLOCK" => new ParsedCommand("OPEN", noun),
            "ASCEND" => new ParsedCommand("CLIMB", noun),
            _ => new ParsedCommand(verb, noun)
        };
    }

    private static bool TryParseDirection(string noun, out Direction direction)
    {
        switch (ItemDefinition.Normalize(noun))
        {
            case "N":
            case "NORTH":
                direction = Direction.North;
                return true;
            case "S":
            case "SOUTH":
                direction = Direction.South;
                return true;
            case "E":
            case "EAST":
                direction = Direction.East;
                return true;
            case "W":
            case "WEST":
                direction = Direction.West;
                return true;
            case "U":
            case "UP":
                direction = Direction.Up;
                return true;
            case "D":
            case "DOWN":
                direction = Direction.Down;
                return true;
            default:
                direction = default;
                return false;
        }
    }

    private string? FindVisibleRoomItem(string noun)
    {
        if (state.CurrentRoom.IsDark && !state.Flags.Contains("lamp-lit"))
        {
            return null;
        }

        return state.CurrentRoom.ItemIds.FirstOrDefault(itemId => state.World.Items[itemId].Matches(noun));
    }

    private string? FindInventoryItem(string noun)
    {
        var exactMatches = state.Inventory.Where(itemId => state.World.Items[itemId].Matches(noun)).ToArray();
        return exactMatches.Length switch
        {
            0 => null,
            1 => exactMatches[0],
            _ => exactMatches.FirstOrDefault(itemId => ItemDefinition.Normalize(state.World.Items[itemId].Name) == ItemDefinition.Normalize(noun))
        };
    }

    private static string FormatExits(IEnumerable<Direction> exits)
    {
        return string.Join(", ", exits.OrderBy(direction => direction).Select(DirectionName));
    }

    private static string DirectionName(Direction direction)
    {
        return direction switch
        {
            Direction.North => "NORTH",
            Direction.South => "SOUTH",
            Direction.East => "EAST",
            Direction.West => "WEST",
            Direction.Up => "UP",
            Direction.Down => "DOWN",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    private static string GetSaveFilePath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), SaveFileName);
    }

    private static void ShowBanner()
    {
        Console.WriteLine("ADVENTURELAND 1976");
        Console.WriteLine("------------------");
        Console.WriteLine("Recover the five lost treasures and return them to the Village Green.");
        Console.WriteLine("Type HELP for the command list.");
    }
}