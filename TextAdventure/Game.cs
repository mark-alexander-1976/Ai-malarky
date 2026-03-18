namespace TextAdventure;

/// <summary>
/// Core game loop and command processor.
/// Implements a classic two-word (verb/noun) parser in the style of Adventureland (1978).
/// </summary>
public class Game
{
    private readonly GameWorld _world;
    private readonly Player _player;

    // Synonyms for directions
    private static readonly Dictionary<string, string> DirectionAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["n"]     = "north",
        ["s"]     = "south",
        ["e"]     = "east",
        ["w"]     = "west",
        ["u"]     = "up",
        ["d"]     = "down",
        ["north"] = "north",
        ["south"] = "south",
        ["east"]  = "east",
        ["west"]  = "west",
        ["up"]    = "up",
        ["down"]  = "down",
    };

    public Game()
    {
        _world  = new GameWorld();
        _player = new Player();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public entry point
    // ─────────────────────────────────────────────────────────────────────────

    public void Run()
    {
        PrintBanner();
        DescribeLocation(true);

        while (_player.IsAlive && !_player.HasWon && !_player.QuitRequested)
        {
            Console.Write("\n> ");
            string? raw = Console.ReadLine();
            if (raw is null) break;          // EOF (piped input, etc.)

            ProcessCommand(raw.Trim());
        }

        if (_player.HasWon)
        {
            PrintWin();
        }
        else if (!_player.IsAlive)
        {
            PrintDeath();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Command dispatcher
    // ─────────────────────────────────────────────────────────────────────────

    private void ProcessCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string verb = parts[0].ToLower();
        string noun = parts.Length > 1 ? string.Join(' ', parts[1..]) : string.Empty;

        // ── Direction shortcuts ──────────────────────────────────────────
        if (DirectionAliases.TryGetValue(verb, out string? dir) && string.IsNullOrEmpty(noun))
        {
            DoMove(dir);
            return;
        }

        switch (verb)
        {
            // Movement
            case "go":
            case "move":
            case "walk":
                if (string.IsNullOrEmpty(noun))
                    Println("Go where?");
                else if (DirectionAliases.TryGetValue(noun, out string? d))
                    DoMove(d);
                else
                    Println("I don't know that direction.");
                break;

            // Look / examine
            case "look":
            case "l":
                if (string.IsNullOrEmpty(noun))
                    DescribeLocation(false);
                else
                    DoExamine(noun);
                break;

            case "examine":
            case "x":
            case "inspect":
                if (string.IsNullOrEmpty(noun))
                    Println("Examine what?");
                else
                    DoExamine(noun);
                break;

            // Take / drop
            case "take":
            case "get":
            case "pick":
                if (string.IsNullOrEmpty(noun))
                    Println("Take what?");
                else
                {
                    // Handle "pick up <item>" by stripping the leading "up "
                    string itemName = noun.StartsWith("up ", StringComparison.OrdinalIgnoreCase)
                        ? noun[3..] : noun;
                    DoTake(itemName);
                }
                break;

            case "drop":
            case "leave":
                if (string.IsNullOrEmpty(noun))
                    Println("Drop what?");
                else
                    DoDrop(noun);
                break;

            // Inventory
            case "inventory":
            case "inv":
            case "i":
                DoInventory();
                break;

            // Use
            case "use":
                if (string.IsNullOrEmpty(noun))
                    Println("Use what?");
                else
                    DoUse(noun);
                break;

            // Fight
            case "fight":
            case "attack":
            case "kill":
            case "slay":
                DoFight(noun);
                break;

            // Read
            case "read":
                DoRead(noun);
                break;

            // Score / health
            case "score":
            case "health":
            case "status":
                Println($"Health: {_player.Health}/100");
                break;

            // Help
            case "help":
            case "h":
            case "?":
                PrintHelp();
                break;

            // Quit
            case "quit":
            case "exit":
            case "q":
                if (Confirm("Are you sure you want to quit? (yes/no) "))
                {
                    _player.QuitRequested = true;
                    Println("\nFarewell, brave adventurer.");
                }
                break;

            default:
                Println("I don't understand that. (Type HELP for a list of commands.)");
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Movement
    // ─────────────────────────────────────────────────────────────────────────

    private void DoMove(string direction)
    {
        Location here = CurrentLocation();

        if (!here.Exits.TryGetValue(direction, out int destId))
        {
            Println("You cannot go that way.");
            return;
        }

        // Check locked exits
        if (here.LockedExits.TryGetValue(direction, out string? requiredItem))
        {
            if (!_player.HasItem(requiredItem))
            {
                PrintLockedMessage(direction, requiredItem, here.Id);
                return;
            }
        }

        // Special: Dragon's Lair north exit only available after dragon defeated
        if (here.Id == 19 && direction == "north" && !_player.DragonDefeated)
        {
            Println("The dragon blocks your path! You must deal with it first.");
            return;
        }

        _player.CurrentLocationId = destId;
        Location dest = CurrentLocation();

        DescribeLocation(true);
        ApplyLocationEffects(dest);
    }

    private static void PrintLockedMessage(string direction, string requiredItem, int locationId)
    {
        string msg = (locationId, direction, requiredItem) switch
        {
            (14, "south", "Rope") =>
                "The river is too fast and deep to cross without some kind of rope.",
            (16, "east", "Iron Key") =>
                "The great gate is locked fast. You need a key to open it.",
            (6,  "down", "Lantern") =>
                "It is pitch black below. Going down without a light would be suicide.",
            _ => $"You need a {requiredItem} to go that way.",
        };
        Println(msg);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Location effects (swamp damage, dragon attack, etc.)
    // ─────────────────────────────────────────────────────────────────────────

    private void ApplyLocationEffects(Location loc)
    {
        if (!loc.IsDangerous) return;

        // Swamp: drains health unless carrying Amulet
        if (loc.Id == 13)
        {
            if (_player.HasItem("Amulet"))
            {
                Println("Your amulet glows warmly, warding off the swamp's malignant influence.");
            }
            else
            {
                _player.Health -= 20;
                Println($"The swamp's foul miasma saps your strength! (Health: {_player.Health}/100)");
                if (!_player.IsAlive)
                    Println("The swamp has claimed you...");
            }
        }

        // Dragon's Lair: instant death without sword
        if (loc.Id == 19 && !_player.DragonDefeated)
        {
            if (!_player.HasItem("Sword"))
            {
                Println("\nThe dragon rears up with a thunderous roar and engulfs you in a torrent of fire!");
                Println("You are slain instantly.");
                _player.Health = 0;
            }
            else
            {
                Println("The dragon eyes you warily, noticing the sword in your hand.");
                Println("(Type FIGHT DRAGON to battle the beast.)");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Look / describe
    // ─────────────────────────────────────────────────────────────────────────

    private void DescribeLocation(bool firstVisit)
    {
        Location here = CurrentLocation();

        // Dark room without light
        if (here.IsDark && !_player.HasLightSource())
        {
            Println($"\n{here.Name}");
            Println(new string('─', here.Name.Length));
            Println("It is pitch black. You cannot see a thing.");
            Println("You could easily stumble into danger without a light source.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Println($"\n{here.Name}");
        Println(new string('─', here.Name.Length));
        Console.ResetColor();

        Println(here.Description);

        // Items on the ground
        if (here.Items.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Println("\nYou can see:");
            foreach (Item item in here.Items)
                Println($"  - {item.Name}");
            Console.ResetColor();
        }

        // Exits
        Console.ForegroundColor = ConsoleColor.Green;
        Println($"\nExits: {string.Join(", ", here.Exits.Keys.Select(k => k.ToUpper()))}");
        Console.ResetColor();

        if (!firstVisit) return;
        here.HasBeenVisited = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Examine
    // ─────────────────────────────────────────────────────────────────────────

    private void DoExamine(string noun)
    {
        // Check inventory first, then the room
        Item? item = _player.GetItem(noun)
                     ?? FindItemInRoom(noun);

        if (item is not null)
        {
            Println(item.Description);
            return;
        }

        // Special scenery keywords
        string key = noun.ToLower();
        Location here = CurrentLocation();

        if (key is "room" or "here" or "around" or "surroundings")
        {
            DescribeLocation(false);
            return;
        }

        if (key == "exits")
        {
            Println($"Exits: {string.Join(", ", here.Exits.Keys.Select(k => k.ToUpper()))}");
            return;
        }

        Println($"You see nothing special about the {noun}.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Take / Drop
    // ─────────────────────────────────────────────────────────────────────────

    private void DoTake(string noun)
    {
        Location here = CurrentLocation();
        Item? item = FindItemInRoom(noun);

        if (item is null)
        {
            if (_player.HasItem(noun))
                Println("You are already carrying that.");
            else
                Println($"There is no {noun} here.");
            return;
        }

        if (item.IsFixed)
        {
            Println($"The {item.Name} cannot be picked up.");
            return;
        }

        here.Items.Remove(item);
        _player.Inventory.Add(item);
        Println($"You take the {item.Name}.");

        // Special reactions
        if (item.Name == "Golden Crown")
        {
            _player.HasWon = true;
        }
    }

    private void DoDrop(string noun)
    {
        Item? item = _player.GetItem(noun);

        if (item is null)
        {
            Println($"You are not carrying a {noun}.");
            return;
        }

        _player.Inventory.Remove(item);
        CurrentLocation().Items.Add(item);
        Println($"You drop the {item.Name}.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inventory
    // ─────────────────────────────────────────────────────────────────────────

    private void DoInventory()
    {
        if (_player.Inventory.Count == 0)
        {
            Println("You are not carrying anything.");
            return;
        }

        Println("You are carrying:");
        foreach (Item item in _player.Inventory)
            Println($"  - {item.Name}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Use
    // ─────────────────────────────────────────────────────────────────────────

    private void DoUse(string noun)
    {
        Item? item = _player.GetItem(noun);

        if (item is null)
        {
            Println($"You are not carrying a {noun}.");
            return;
        }

        Location here = CurrentLocation();

        switch (item.Name)
        {
            case "Lantern":
                if (here.IsDark)
                    Println("You hold the lantern aloft. Its warm light reveals the location in full.");
                else
                    Println("The lantern is already lit. It burns steadily.");
                break;

            case "Torch":
                if (here.IsDark)
                    Println("You hold the torch high. Its flickering light illuminates the darkness.");
                else
                    Println("The torch burns brightly.");
                break;

            case "Rope":
                if (here.Id == 14)
                    Println("You tie the rope to the bridge pillars, creating a way across. (Now you can go SOUTH.)");
                else
                    Println("You uncoil the rope but there is nothing useful to attach it to here.");
                break;

            case "Iron Key":
                if (here.Id == 16)
                    Println("You insert the iron key into the castle gate lock. It turns with a heavy clunk. (Go EAST to enter.)");
                else
                    Println("There is no lock here that this key fits.");
                break;

            case "Amulet":
                Println("The jade amulet glows with a soft protective light as you hold it.");
                break;

            case "Map":
                PrintMap();
                break;

            case "Water Flask":
                if (_player.Health < 100)
                {
                    _player.Health = Math.Min(100, _player.Health + 20);
                    Println($"You drink from the flask. The cool water revives you. (Health: {_player.Health}/100)");
                }
                else
                    Println("You sip from the flask. The water is refreshing.");
                break;

            case "Gold Coin":
                Println("You flip the gold coin. Heads. Not that it helps right now.");
                break;

            case "Sword":
                if (here.Id == 19 && !_player.DragonDefeated)
                    Println("You brandish the sword at the dragon. (Type FIGHT DRAGON to attack!)");
                else
                    Println("You wave the sword in the air. It makes a satisfying whoosh.");
                break;

            default:
                Println($"You're not sure how to use the {item.Name} here.");
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Fight
    // ─────────────────────────────────────────────────────────────────────────

    private void DoFight(string noun)
    {
        Location here = CurrentLocation();

        if (here.Id != 19)
        {
            Println("There is nothing to fight here.");
            return;
        }

        bool targetingDragon = string.IsNullOrEmpty(noun) ||
                               noun.Equals("dragon", StringComparison.OrdinalIgnoreCase);

        if (!targetingDragon)
        {
            Println("There is no such enemy here.");
            return;
        }

        if (_player.DragonDefeated)
        {
            Println("The dragon is already defeated. Its huge carcass lies on the cave floor.");
            return;
        }

        if (!_player.HasItem("Sword"))
        {
            Println("You attack the dragon bare-handed. It's not impressed.");
            Println("The dragon's tail sweeps you across the cave with bone-crushing force.");
            _player.Health = 0;
            return;
        }

        // Epic fight sequence
        Println("\nYou raise the sword and charge the dragon with a battle cry!");
        Println("The beast spews a cone of flame – you dive aside, singeing your cloak.");
        Println("Slashing upward, your blade finds a gap in its scales!");
        Println("The dragon thrashes wildly – you leap back as its claws rend the stone floor.");
        Println("With a final desperate thrust you drive the sword home.");
        Println("\nThe great dragon lets out a thunderous death roar and crashes to the ground.");
        Println("The cave shakes. Then... silence.");
        Println("\nA passage to the NORTH is now open.");

        _player.DragonDefeated = true;
        _player.Health -= 10;  // You got a bit singed
        // Unlock north exit
        here.Exits["north"] = 20;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Println($"\n*** You slew the dragon! (Health: {_player.Health}/100) ***");
        Console.ResetColor();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Read
    // ─────────────────────────────────────────────────────────────────────────

    private void DoRead(string noun)
    {
        Item? item = _player.GetItem(noun) ?? FindItemInRoom(noun);

        if (item is null)
        {
            Println($"You don't see a {noun} to read.");
            return;
        }

        switch (item.Name)
        {
            case "Map":
                PrintMap();
                break;

            default:
                Println($"There is nothing written on the {item.Name}.");
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private Location CurrentLocation() => _world.GetLocation(_player.CurrentLocationId);

    private Item? FindItemInRoom(string noun) =>
        CurrentLocation().Items.FirstOrDefault(
            i => i.Name.Equals(noun, StringComparison.OrdinalIgnoreCase) ||
                 i.Name.Contains(noun, StringComparison.OrdinalIgnoreCase));

    private static void Println(string text) => Console.WriteLine(text);

    private static bool Confirm(string prompt)
    {
        Console.Write(prompt);
        string? answer = Console.ReadLine();
        return answer?.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) == true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Presentation
    // ─────────────────────────────────────────────────────────────────────────

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════╗
║          T H E   L O S T   K I N G D O M                     ║
║                A Text Adventure                              ║
╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.WriteLine("\nIn the age before memory, a great kingdom fell.");
        Console.WriteLine("Its legendary Golden Crown was lost in the chaos.");
        Console.WriteLine("You are an adventurer seeking fame and fortune.");
        Console.WriteLine("Find the Golden Crown and claim it as your own!\n");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Type HELP for a list of commands.\n");
        Console.ResetColor();
    }

    private static void PrintHelp()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("""

  ┌─ COMMANDS ──────────────────────────────────────────────────┐
  │  LOOK / L              - Describe your surroundings         │
  │  EXAMINE / X <thing>   - Examine an item or object          │
  │  GO <dir>  or  N/S/E/W/U/D  - Move in a direction           │
  │  TAKE / GET <item>     - Pick up an item                    │
  │  DROP <item>           - Put an item down                   │
  │  INVENTORY / I         - List what you are carrying         │
  │  USE <item>            - Use an item                        │
  │  READ <item>           - Read something                     │
  │  FIGHT DRAGON          - Attack a foe                       │
  │  HEALTH / STATUS       - Show your current health           │
  │  HELP / ?              - Show this help                     │
  │  QUIT / Q              - Quit the game                      │
  └─────────────────────────────────────────────────────────────┘
  Tip: Most commands accept short forms and synonyms.
  Tip: You'll need light to see in dark places.
""");
        Console.ResetColor();
    }

    private static void PrintMap()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("""

  ╔══════════════ MAP OF THE LOST KINGDOM ══════════════════════╗
  ║                                                             ║
  ║  [Summit Peak]                                              ║
  ║       │                                                     ║
  ║  [Mountain Trail]──[Hidden Cave]                            ║
  ║       │                   │                                 ║
  ║  [Dark Forest]──[Ancient Ruins]   [Underground Lake]        ║
  ║       │               │                                     ║
  ║  [Forest Clearing]──[Rocky Path]──[Desert Oasis]            ║
  ║       │                               │                     ║
  ║  [Riverbank]──[Muddy Swamp]   [Abandoned Village]           ║
  ║       │                        │             │              ║
  ║  [Stone Bridge]         [Marketplace]   [Old Tavern]        ║
  ║       │                                                     ║
  ║  [Castle Gates]──[Castle Courtyard]──[Castle Armory]        ║
  ║                        │                                    ║
  ║                  [Dragon's Lair]                            ║
  ║                        │                                    ║
  ║                  [Treasure Vault]  ← YOUR GOAL              ║
  ╚═════════════════════════════════════════════════════════════╝
""");
        Console.ResetColor();
    }

    private static void PrintWin()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("""

╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║   *** Y O U   W I N ! ***                                    ║
║                                                              ║
║   You have claimed the Golden Crown of the Ancient Kingdom!  ║
║                                                              ║
║   Bards across the land will sing of your courage for        ║
║   a thousand years.                                          ║
║                                                              ║
║   Well played, adventurer!                                   ║
╚══════════════════════════════════════════════════════════════╝
""");
        Console.ResetColor();
    }

    private static void PrintDeath()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("""

╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║   *** Y O U   A R E   D E A D ***                            ║
║                                                              ║
║   Your quest ends here.                                      ║
║   Perhaps another adventurer will complete what you began.   ║
║                                                              ║
║   Game over.                                                 ║
╚══════════════════════════════════════════════════════════════╝
""");
        Console.ResetColor();
    }
}
