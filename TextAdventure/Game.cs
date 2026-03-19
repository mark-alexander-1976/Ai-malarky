using System.Text.Json;

sealed class Game
{
    private const string DefaultSaveFileName = "adventureland-save.json";
    private static readonly JsonSerializerOptions SaveOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly Random random;
    private readonly string saveDirectory;
    private GameState state;

    public Game()
        : this(null)
    {
    }

    internal Game(string? saveDirectory)
        : this(saveDirectory, null)
    {
    }

    internal Game(string? saveDirectory, Random? random)
    {
        this.random = random ?? Random.Shared;
        this.saveDirectory = string.IsNullOrWhiteSpace(saveDirectory)
            ? Directory.GetCurrentDirectory()
            : saveDirectory;
        state = CreateNewState();
    }

    internal GameState CurrentState => state;

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

    internal static ParsedCommand ParseForTesting(string input)
    {
        return Parse(input);
    }

    internal static GameState CreateNewStateForTesting()
    {
        return CreateNewState();
    }

    internal void SaveGameForTesting(string? slotName)
    {
        SaveGame(slotName);
    }

    internal void LoadGameForTesting(string? slotName)
    {
        LoadGame(slotName);
    }

    internal string GetSavePathForTesting(string? slotName)
    {
        return GetSaveFilePath(slotName);
    }

    internal IReadOnlyList<string> ListSaveSlotsForTesting()
    {
        return ListSaveSlots();
    }

    internal void DeleteSaveForTesting(string? slotName)
    {
        DeleteSave(slotName);
    }

    internal void ConfirmDeleteSaveForTesting(string? slotName)
    {
        ConfirmDeleteSave(slotName);
    }

    internal void RenameSaveForTesting(string? noun)
    {
        RenameSave(noun);
    }

    internal void SecureAllTreasuresForTesting()
    {
        foreach (var treasureId in state.World.TreasureIds)
        {
            state.SecuredTreasures.Add(treasureId);
        }
    }

    internal void ForceSquirrelEncounterForTesting(int squirrelLevel)
    {
        state.ActiveSquirrelLevel = squirrelLevel;
    }

    internal void AttackSquirrelForTesting()
    {
        HandleAttack("squirrel");
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
        if (state.ActiveSquirrelLevel.HasValue && !CanExecuteDuringSquirrelEncounter(command.Verb))
        {
            Console.WriteLine("A savage squirrel bars your path. ATTACK SQUIRREL first.");
            return;
        }

        switch (command.Verb)
        {
            case "":
                Console.WriteLine("Speak up.");
                return;
            case "LOOK":
                DescribeCurrentRoom(forceFullDescription: true);
                return;
            case "HELP":
                ShowHelp(command.Noun);
                return;
            case "INVENTORY":
                ShowInventory();
                return;
            case "SCORE":
                ShowScore();
                return;
            case "SAVE":
                SaveGame(command.Noun);
                return;
            case "LOAD":
                LoadGame(command.Noun);
                return;
            case "LISTSAVES":
                HandleListSaves();
                return;
            case "DELETE":
                DeleteSave(command.Noun);
                return;
            case "CONFIRMDELETE":
                ConfirmDeleteSave(command.Noun);
                return;
            case "RENAMESAVE":
                RenameSave(command.Noun);
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
            case "PUSH":
                HandlePush(command.Noun);
                return;
            case "PULL":
                HandlePull(command.Noun);
                return;
            case "LISTEN":
                HandleListen(command.Noun);
                return;
            case "SEARCH":
                HandleSearch(command.Noun);
                return;
            case "ATTACK":
                HandleAttack(command.Noun);
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
        MaybeTriggerSquirrelEncounter();
    }

    private void MaybeTriggerSquirrelEncounter()
    {
        if (state.ActiveSquirrelLevel.HasValue || !AreSquirrelTrialsActive())
        {
            return;
        }

        if (random.Next(4) != 0)
        {
            return;
        }

        state.ActiveSquirrelLevel = state.SquirrelsDefeated + 1;
        Console.WriteLine($"A furious squirrel of level {state.ActiveSquirrelLevel.Value} leaps from hiding. ATTACK SQUIRREL to defeat it.");
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
                if (TryCompleteAdventure())
                {
                    Console.WriteLine("As the last treasure touches the plinth, a bell begins to toll across the valley.");
                }
                else
                {
                    Console.WriteLine($"The plinth shudders and five squirrel trials awaken. Defeat {state.RequiredSquirrelTrials - state.SquirrelsDefeated} squirrel foes before the land will accept your victory.");
                }
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

    private void HandlePush(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("PUSH what?");
            return;
        }

        var target = ItemDefinition.Normalize(noun);

        if (target is "DOOR" or "GRATE" or "GATE")
        {
            if (state.CurrentRoomId is "old-well" or "sunken-stair")
            {
                Console.WriteLine(state.Flags.Contains("grate-unlocked")
                    ? "The unlocked grate swings under your weight."
                    : "The locked grate refuses to move.");
                return;
            }

            if (state.CurrentRoomId is "moon-tower" or "rope-chasm")
            {
                Console.WriteLine(state.Flags.Contains("vault-unlocked")
                    ? "The moon door gives way when you shove it."
                    : "The moon door will not yield to force.");
                return;
            }
        }

        if (target is "PLINTH" or "STONE")
        {
            Console.WriteLine("The ancient stone does not budge.");
            return;
        }

        Console.WriteLine("It does not respond to pushing.");
    }

    private void HandlePull(string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("PULL what?");
            return;
        }

        var target = ItemDefinition.Normalize(noun);

        if (target == "ROPE")
        {
            if (state.Flags.Contains("rope-secured") && state.CurrentRoomId is "underground-lake" or "rope-chasm")
            {
                Console.WriteLine("The rope is stretched tight and holds firm.");
                return;
            }

            if (state.Inventory.Contains("rope") || FindVisibleRoomItem(noun) == "rope")
            {
                Console.WriteLine("The rope slips through your hands with a dry rasp.");
                return;
            }
        }

        if (target is "CHAIN" or "LEVER")
        {
            Console.WriteLine("You find no hidden mechanism answering your pull.");
            return;
        }

        Console.WriteLine("Nothing happens.");
    }

    private void HandleListen(string? noun)
    {
        _ = noun;

        var message = state.CurrentRoomId switch
        {
            "village-green" => "You hear distant crows and the faint creak of village signboards.",
            "whispering-glen" => "Water chatters softly through reeds somewhere nearby.",
            "crypt-entry" => "Dripping water echoes in the dark like slow footsteps.",
            "underground-lake" => "Black water laps at stone, and a low draft moans to the east.",
            "serpent-shrine" => "The shrine is very still, save for the hollow echo of your own breath.",
            "moon-tower" => "Wind circles the tower top in long, lonely sighs.",
            _ => "You pause and listen, but hear nothing of immediate use."
        };

        Console.WriteLine(message);
    }

    private void HandleSearch(string? noun)
    {
        if (state.CurrentRoom.IsDark && !state.Flags.Contains("lamp-lit"))
        {
            Console.WriteLine("You fumble in the dark and find nothing certain.");
            return;
        }

        var target = string.IsNullOrWhiteSpace(noun) ? "ROOM" : ItemDefinition.Normalize(noun);

        if (target is "ROOM" or "HERE")
        {
            if (state.CurrentRoom.ItemIds.Count > 0)
            {
                PrintVisibleItems(state.CurrentRoom);
                return;
            }

            Console.WriteLine(state.CurrentRoomId switch
            {
                "wizard-study" => "Among the dust you notice moon symbols worn smooth by many hands.",
                "lantern-hut" => "A careful search turns up old oil stains and cold ashes, but nothing beyond the lamp.",
                "old-well" => "You find only old stonework, iron bars, and the stubborn lock below.",
                _ => "After a careful search you find nothing new."
            });
            return;
        }

        if (state.CurrentRoomId == "wizard-study" && target is "BOOKS" or "BOOK" or "SHELVES")
        {
            Console.WriteLine("Behind a row of swollen books you discover a brittle note about the moon door and its twin below the chasm.");
            return;
        }

        if (state.CurrentRoomId == "lantern-hut" && target is "SHELVES" or "RAGS")
        {
            Console.WriteLine("You stir up dust and find nothing but oilcloth scraps and an empty tin.");
            return;
        }

        Console.WriteLine("You search carefully but uncover nothing of value.");
    }

    private void HandleAttack(string? noun)
    {
        if (!state.ActiveSquirrelLevel.HasValue)
        {
            Console.WriteLine("You lash out at empty air.");
            return;
        }

        if (string.IsNullOrWhiteSpace(noun))
        {
            Console.WriteLine("ATTACK what?");
            return;
        }

        if (ItemDefinition.Normalize(noun) is not "SQUIRREL" and not "MAD SQUIRREL" and not "BEAST" and not "CREATURE")
        {
            Console.WriteLine("The squirrel is your immediate problem.");
            return;
        }

        var squirrelLevel = state.ActiveSquirrelLevel.Value;
        state.ActiveSquirrelLevel = null;
        state.SquirrelsDefeated++;
        state.Moves++;

        Console.WriteLine($"You defeat the level {squirrelLevel} squirrel after a brief, furious struggle.");
        Console.WriteLine($"Squirrels defeated: {state.SquirrelsDefeated}/{state.RequiredSquirrelTrials}.");

        if (AreSquirrelTrialsActive())
        {
            Console.WriteLine("More squirrel trials remain before the end can truly come.");
            return;
        }

        if (TryCompleteAdventure())
        {
            Console.WriteLine("With the final squirrel defeated, the last barrier to victory falls away.");
        }
    }

    private void HandleListSaves()
    {
        var slots = ListSaveSlots();
        if (slots.Count == 0)
        {
            Console.WriteLine("No save slots are present.");
            return;
        }

        Console.WriteLine($"Save slots: {string.Join(", ", slots)}.");
    }

    private void ConfirmDeleteSave(string? slotName)
    {
        var normalizedSlot = NormalizeSlotName(ExtractSlotName(slotName));
        var saveFilePath = GetSaveFilePath(normalizedSlot);

        if (!File.Exists(saveFilePath))
        {
            Console.WriteLine($"There is no saved game in slot '{normalizedSlot}'.");
            return;
        }

        File.Delete(saveFilePath);
        Console.WriteLine($"Deleted save slot '{normalizedSlot}'.");
    }

    private void RenameSave(string? noun)
    {
        if (!TryParseRenameSlots(noun, out var oldSlot, out var newSlot, out var errorMessage))
        {
            Console.WriteLine(errorMessage);
            return;
        }

        var oldNormalized = NormalizeSlotName(oldSlot);
        var newNormalized = NormalizeSlotName(newSlot);
        var oldPath = GetSaveFilePath(oldNormalized);
        var newPath = GetSaveFilePath(newNormalized);

        if (!File.Exists(oldPath))
        {
            Console.WriteLine($"There is no saved game in slot '{oldNormalized}'.");
            return;
        }

        if (File.Exists(newPath))
        {
            Console.WriteLine($"A save slot named '{newNormalized}' already exists.");
            return;
        }

        File.Move(oldPath, newPath);
        Console.WriteLine($"Renamed save slot '{oldNormalized}' to '{newNormalized}'.");
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

    private void SaveGame(string? slotName)
    {
        var normalizedSlot = NormalizeSlotName(slotName);
        var saveFilePath = GetSaveFilePath(slotName);
        var saveData = new SaveData
        {
            CurrentRoomId = state.CurrentRoomId,
            Inventory = state.Inventory.OrderBy(item => item).ToList(),
            Flags = state.Flags.OrderBy(flag => flag).ToList(),
            SecuredTreasures = state.SecuredTreasures.OrderBy(item => item).ToList(),
            SquirrelsDefeated = state.SquirrelsDefeated,
            ActiveSquirrelLevel = state.ActiveSquirrelLevel,
            Moves = state.Moves,
            RoomItems = state.World.Rooms.Values.ToDictionary(
                room => room.Id,
                room => room.ItemIds.OrderBy(item => item).ToList(),
                StringComparer.OrdinalIgnoreCase)
        };

        File.WriteAllText(saveFilePath, JsonSerializer.Serialize(saveData, SaveOptions));
        Console.WriteLine($"Game saved in slot '{normalizedSlot}' to {saveFilePath}.");
    }

    private void LoadGame(string? slotName)
    {
        var normalizedSlot = NormalizeSlotName(slotName);
        var saveFilePath = GetSaveFilePath(slotName);
        if (!File.Exists(saveFilePath))
        {
            Console.WriteLine($"There is no saved game in slot '{normalizedSlot}'.");
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

        Console.WriteLine($"Game loaded from slot '{normalizedSlot}'.");
        DescribeCurrentRoom(forceFullDescription: true);
    }

    private void DeleteSave(string? slotName)
    {
        var normalizedSlot = NormalizeSlotName(ExtractSlotName(slotName));
        var saveFilePath = GetSaveFilePath(normalizedSlot);

        if (!File.Exists(saveFilePath))
        {
            Console.WriteLine($"There is no saved game in slot '{normalizedSlot}'.");
            return;
        }

        if (normalizedSlot == "default")
        {
            Console.WriteLine("The default slot is protected. Use CONFIRM DELETE or CONFIRM DELETE DEFAULT to remove it.");
            return;
        }

        File.Delete(saveFilePath);
        Console.WriteLine($"Deleted save slot '{normalizedSlot}'.");
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
        restoredState.SquirrelsDefeated = saveData.SquirrelsDefeated;
        restoredState.ActiveSquirrelLevel = saveData.ActiveSquirrelLevel;

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

        if (state.ActiveSquirrelLevel.HasValue)
        {
            Console.WriteLine($"A level {state.ActiveSquirrelLevel.Value} squirrel crouches here, ready to spring.");
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
                if (state.SecuredTreasures.Count == state.World.TreasureIds.Count && state.SquirrelsDefeated < state.RequiredSquirrelTrials)
                {
                    Console.WriteLine($"The final squirrel trials await: {state.SquirrelsDefeated}/{state.RequiredSquirrelTrials} defeated.");
                }
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
        Console.WriteLine($"Squirrel trials cleared: {state.SquirrelsDefeated}/{state.RequiredSquirrelTrials}.");
    }

    private void ShowHelp(string? noun)
    {
        if (ItemDefinition.Normalize(noun ?? string.Empty) is "SAVES" or "SAVE" or "SAVE SLOTS" or "SLOTS")
        {
            ShowSaveHelp();
            return;
        }

        Console.WriteLine("Commands: LOOK, GO NORTH, GO SOUTH, GO EAST, GO WEST, GO UP, GO DOWN,");
        Console.WriteLine("GET item, DROP item, EXAMINE item, USE item, READ object, OPEN object,");
        Console.WriteLine("CLIMB, PUSH object, PULL object, LISTEN, SEARCH, ATTACK squirrel, INVENTORY,");
        Console.WriteLine("SCORE, SAVE [slot], LOAD [slot], LIST SAVES, DELETE [slot], RENAME SAVE,");
        Console.WriteLine("HELP SAVES, HELP, QUIT.");
        Console.WriteLine("Short movement commands: N, S, E, W, U, D.");
    }

    private static void ShowSaveHelp()
    {
        Console.WriteLine("SAVE MANAGEMENT");
        Console.WriteLine("SAVE                save to the default slot");
        Console.WriteLine("LOAD                load the default slot");
        Console.WriteLine("SAVE chapter one    save to a named slot");
        Console.WriteLine("LOAD chapter one    load a named slot");
        Console.WriteLine("LIST SAVES          list all available slots");
        Console.WriteLine("DELETE chapter one  delete a named slot");
        Console.WriteLine("CONFIRM DELETE      delete the protected default slot");
        Console.WriteLine("RENAME SAVE old new rename single-word slots");
        Console.WriteLine("RENAME SAVE old TO new   rename multi-word slots safely");
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
            "HEAR" => new ParsedCommand("LISTEN", noun),
            "INSPECT" => new ParsedCommand("SEARCH", noun),
            "FIGHT" => new ParsedCommand("ATTACK", noun),
            "KILL" => new ParsedCommand("ATTACK", noun),
            "HIT" => new ParsedCommand("ATTACK", noun),
            "LIST" when ItemDefinition.Normalize(noun ?? string.Empty) is "SAVES" or "SAVE SLOTS" or "SLOTS" => new ParsedCommand("LISTSAVES", null),
            "DELETE" => new ParsedCommand("DELETE", noun),
            "CONFIRM" when ItemDefinition.Normalize(noun ?? string.Empty) is "DELETE" or "DELETE DEFAULT" or "DELETE SAVE" or "DELETE SAVE DEFAULT" => new ParsedCommand("CONFIRMDELETE", noun),
            "RENAME" when !string.IsNullOrWhiteSpace(noun) && ItemDefinition.Normalize(noun).StartsWith("SAVE ", StringComparison.OrdinalIgnoreCase) => new ParsedCommand("RENAMESAVE", noun),
            "ERASE" => new ParsedCommand("DELETE", noun),
            "REMOVE" => new ParsedCommand("DELETE", noun),
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

    private string GetSaveFilePath(string? slotName)
    {
        var normalizedSlot = NormalizeSlotName(slotName);
        var fileName = normalizedSlot == "default"
            ? DefaultSaveFileName
            : $"adventureland-save-{normalizedSlot}.json";
        return Path.Combine(saveDirectory, fileName);
    }

    private List<string> ListSaveSlots()
    {
        var slots = new List<string>();
        var defaultPath = Path.Combine(saveDirectory, DefaultSaveFileName);
        if (File.Exists(defaultPath))
        {
            slots.Add("default");
        }

        if (!Directory.Exists(saveDirectory))
        {
            return slots;
        }

        const string prefix = "adventureland-save-";
        const string suffix = ".json";

        foreach (var filePath in Directory.EnumerateFiles(saveDirectory, "adventureland-save-*.json"))
        {
            var fileName = Path.GetFileName(filePath);
            if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || !fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var slot = fileName.Substring(prefix.Length, fileName.Length - prefix.Length - suffix.Length);
            if (!string.IsNullOrWhiteSpace(slot))
            {
                slots.Add(slot);
            }
        }

        slots.Sort(StringComparer.OrdinalIgnoreCase);
        return slots;
    }

    private static string? ExtractSlotName(string? slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
        {
            return slotName;
        }

        var normalized = ItemDefinition.Normalize(slotName);
        if (normalized.StartsWith("SAVE ", StringComparison.OrdinalIgnoreCase))
        {
            return slotName.Substring(slotName.IndexOf(' ') + 1);
        }

        if (normalized.StartsWith("SLOT ", StringComparison.OrdinalIgnoreCase))
        {
            return slotName.Substring(slotName.IndexOf(' ') + 1);
        }

        return slotName;
    }

    private static bool TryParseRenameSlots(string? noun, out string? oldSlot, out string? newSlot, out string errorMessage)
    {
        oldSlot = null;
        newSlot = null;

        if (string.IsNullOrWhiteSpace(noun))
        {
            errorMessage = "RENAME SAVE requires an old slot and a new slot.";
            return false;
        }

        var working = noun.Trim();
        if (ItemDefinition.Normalize(working).StartsWith("SAVE ", StringComparison.OrdinalIgnoreCase))
        {
            working = working.Substring(working.IndexOf(' ') + 1).Trim();
        }

        var markerIndex = working.IndexOf(" TO ", StringComparison.OrdinalIgnoreCase);
        if (markerIndex >= 0)
        {
            oldSlot = working.Substring(0, markerIndex).Trim();
            newSlot = working.Substring(markerIndex + 4).Trim();
        }
        else
        {
            var parts = working.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                errorMessage = "RENAME SAVE requires both an old slot and a new slot.";
                return false;
            }

            oldSlot = parts[0];
            newSlot = string.Join(' ', parts.Skip(1));
        }

        if (string.IsNullOrWhiteSpace(oldSlot) || string.IsNullOrWhiteSpace(newSlot))
        {
            errorMessage = "RENAME SAVE requires both an old slot and a new slot.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static string NormalizeSlotName(string? slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
        {
            return "default";
        }

        var builder = new List<char>(slotName.Length);

        foreach (var character in slotName.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character) || character == ' ' || character == '-' || character == '_')
            {
                builder.Add(character);
            }
        }

        var normalized = string.Join(' ', new string(builder.ToArray()).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Replace(' ', '-');

        return string.IsNullOrWhiteSpace(normalized) ? "default" : normalized;
    }

    private static void ShowBanner()
    {
        Console.WriteLine("ADVENTURELAND 1976");
        Console.WriteLine("------------------");
        Console.WriteLine("Recover the five lost treasures and return them to the Village Green.");
        Console.WriteLine("Five random squirrel trials stand between you and the true ending.");
        Console.WriteLine("Type HELP for the command list.");
    }

    private bool CanExecuteDuringSquirrelEncounter(string verb)
    {
        return verb is "LOOK" or "HELP" or "INVENTORY" or "SCORE" or "SAVE" or "LOAD" or "LISTSAVES" or "DELETE" or "CONFIRMDELETE" or "RENAMESAVE" or "QUIT" or "EXAMINE" or "ATTACK";
    }

    private bool AreSquirrelTrialsActive()
    {
        return state.SecuredTreasures.Count == state.World.TreasureIds.Count && state.SquirrelsDefeated < state.RequiredSquirrelTrials;
    }

    private bool TryCompleteAdventure()
    {
        if (state.SecuredTreasures.Count == state.World.TreasureIds.Count && state.SquirrelsDefeated >= state.RequiredSquirrelTrials)
        {
            state.Won = true;
            return true;
        }

        return false;
    }
}