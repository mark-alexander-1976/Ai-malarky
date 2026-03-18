namespace TextAdventure;

/// <summary>
/// Core game loop and command processor.
/// Implements a classic two-word (verb/noun) parser in the style of Adventureland (1978).
/// </summary>
public class Game
{
    private readonly GameWorld _world;
    private readonly Player _player;
    private static readonly Random _random = new();
    private readonly HashSet<int> _squirrelLocationIds = [21, 22, 23, 24, 25];
    private int _activeSquirrelLocationId;

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

        if (IsSquirrelLocation(here.Id) && direction == "north" && !_player.SquirrelLocationsCleared.Contains(here.Id))
        {
            if (_activeSquirrelLocationId != here.Id)
            {
                _activeSquirrelLocationId = here.Id;
                Println(GetForcedSquirrelEncounterText(here.Id));
            }
            else
            {
                Println("The squirrel blocks your path! (Type FIGHT SQUIRREL to drive it off.)");
            }
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

            return;
        }

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

            return;
        }

        if (IsSquirrelLocation(loc.Id) && !_player.SquirrelLocationsCleared.Contains(loc.Id))
        {
            if (_activeSquirrelLocationId == loc.Id)
            {
                Println($"{GetSquirrelName(loc.Id)} is still here, glaring at you menacingly. (Type FIGHT SQUIRREL.)");
                return;
            }

            if (_random.Next(2) == 0)
            {
                _activeSquirrelLocationId = loc.Id;
                Println(GetRandomSquirrelEncounterText(loc.Id));
            }
            else
            {
                Println("You feel like you're being watched. Something lurks in the branches above...");
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
                else if (IsSquirrelLocation(here.Id) && _activeSquirrelLocationId == here.Id && !_player.SquirrelLocationsCleared.Contains(here.Id))
                    Println("You brandish the sword at the squirrel. (Type FIGHT SQUIRREL to attack!)");
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
        bool targetingDragon = here.Id == 19 &&
                               (string.IsNullOrEmpty(noun) || noun.Equals("dragon", StringComparison.OrdinalIgnoreCase));
        bool targetingSquirrel = IsSquirrelLocation(here.Id) &&
                                 (string.IsNullOrEmpty(noun) ||
                                  noun.Equals("squirrel", StringComparison.OrdinalIgnoreCase) ||
                                  noun.Equals("squirrel king", StringComparison.OrdinalIgnoreCase));

        if (targetingDragon)
        {
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

            Println("\nYou raise the sword and charge the dragon with a battle cry!");
            Println("The beast spews a cone of flame – you dive aside, singeing your cloak.");
            Println("Slashing upward, your blade finds a gap in its scales!");
            Println("The dragon thrashes wildly – you leap back as its claws rend the stone floor.");
            Println("With a final desperate thrust you drive the sword home.");
            Println("\nThe great dragon lets out a thunderous death roar and crashes to the ground.");
            Println("The cave shakes. Then... silence.");
            Println("\nA passage to the NORTH is now open.");

            _player.DragonDefeated = true;
            _player.Health -= 10;
            here.Exits["north"] = 21;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Println($"\n*** You slew the dragon! (Health: {_player.Health}/100) ***");
            Console.ResetColor();
            return;
        }

        if (targetingSquirrel)
        {
            DoFightSquirrel(here);
            return;
        }

        if (here.Id == 19 || IsSquirrelLocation(here.Id))
        {
            Println("There is no such enemy here.");
            return;
        }

        Println("There is nothing to fight here.");
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

    private bool IsSquirrelLocation(int locationId) => _squirrelLocationIds.Contains(locationId);

    private static string GetSquirrelName(int locationId) => locationId switch
    {
        21 => "the grey squirrel",
        22 => "the fat brown squirrel",
        23 => "the furious red squirrel",
        24 => "the rabid black squirrel",
        25 => "the Squirrel King",
        _ => "the squirrel",
    };

    private static string GetRandomSquirrelEncounterText(int locationId) => locationId switch
    {
        21 => "A grey squirrel drops from a root above and lands squarely in your path, chittering like an outraged sentry!",
        22 => "An enormous brown squirrel rolls out of the acorns, stuffs one into its cheeks, and squares up for battle!",
        23 => "A red squirrel hurtles down the inside of the trunk in a blur of claws and tail, shrieking a challenge!",
        24 => "A black squirrel bursts from the leaves with wild eyes and a pine cone clutched like a grenade!",
        25 => "Trumpeting squeaks echo through the chamber as the Squirrel King springs onto his acorn throne and points a bottle-cap blade at you!",
        _ => "A squirrel leaps from hiding and blocks your way!",
    };

    private static string GetForcedSquirrelEncounterText(int locationId) => locationId switch
    {
        21 => "A grey squirrel leaps from the roots and blocks the northern burrow! (Type FIGHT SQUIRREL.)",
        22 => "A fat brown squirrel skids across the acorns and plants itself in your way! (Type FIGHT SQUIRREL.)",
        23 => "A furious red squirrel scampers down the hollow trunk and cuts off your path! (Type FIGHT SQUIRREL.)",
        24 => "A rabid black squirrel dives from the canopy and lands between you and the bridge! (Type FIGHT SQUIRREL.)",
        25 => "The Squirrel King rises in righteous fury and bars the way to the vault! (Type FIGHT SQUIRREL.)",
        _ => "A squirrel blocks your path! (Type FIGHT SQUIRREL.)",
    };

    private void DoFightSquirrel(Location here)
    {
        if (_player.SquirrelLocationsCleared.Contains(here.Id))
        {
            Println("You've already defeated the squirrel here.");
            return;
        }

        if (_activeSquirrelLocationId != here.Id)
        {
            Println("There's no squirrel threatening you right now.");
            return;
        }

        bool hasSword = _player.HasItem("Sword");
        int damageTaken = (here.Id, hasSword) switch
        {
            (21, true) => 5,
            (21, false) => 15,
            (22, true) => 5,
            (22, false) => 15,
            (23, true) => 5,
            (23, false) => 20,
            (24, true) => 10,
            (24, false) => 20,
            (25, true) => 15,
            (25, false) => 30,
            _ => hasSword ? 5 : 15,
        };

        switch (here.Id)
        {
            case 21:
                Println("\nYou lunge at the grey squirrel as it chitters an alarm.");
                Println(hasSword
                    ? "One quick sweep of your sword parts whiskers from pride, and the creature flees into the roots."
                    : "You grapple the furious little beast bare-handed and finally hurl it into a pile of nut shells.");
                break;

            case 22:
                Println("\nThe fat brown squirrel barrels across the grove like a furry boulder.");
                Println(hasSword
                    ? "You sidestep, tap it smartly with the flat of your blade, and send it tumbling into the acorns."
                    : "It batters your shins and climbs your cloak before you shake it loose with a desperate roar.");
                break;

            case 23:
                Println("\nThe furious red squirrel spirals around the trunk so fast it seems to be in three places at once.");
                Println(hasSword
                    ? "You time the motion, slash through a rain of bark, and force it squealing back into a knot-hole."
                    : "You swat, duck, and endure a storm of claws before pinning it long enough to send it scampering away.");
                break;

            case 24:
                Println("\nThe rabid black squirrel hurls pine cones with terrifying accuracy as branches sway underfoot.");
                Println(hasSword
                    ? "You bat the missiles aside and drive forward until it loses nerve and vanishes into the leaves."
                    : "You stumble through the barrage, catch one final dive with your forearm, and fling the beast into the canopy.");
                break;

            case 25:
                Println("\nThe Squirrel King rises upon his acorn throne, bottle-cap crown gleaming and twig sceptre held high.");
                Println("He unleashes a shrill decree that sounds suspiciously like an insult to your footwear.");
                Println(hasSword
                    ? "You parry his tiny bottle-cap sabre, endure a hail of ceremonial acorns, and swat his throne clean over with a heroic flourish."
                    : "The royal tyrant launches himself at your face, claws flailing, and you wrestle him across the chamber in utter indignity.");
                Println("With one final indignant squeak, the Squirrel King is chased off by his own tumbling pine-cone reserves.");
                break;
        }

        _player.Health -= damageTaken;
        _player.SquirrelLocationsCleared.Add(here.Id);
        _activeSquirrelLocationId = 0;

        if (!_player.IsAlive)
        {
            Println($"You defeat {GetSquirrelName(here.Id)}, but your wounds are too much to bear. (Health: {_player.Health}/100)");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Println($"\n*** You defeated {GetSquirrelName(here.Id)}! (Health: {_player.Health}/100) ***");
        Console.ResetColor();
    }

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
    │  FIGHT <enemy>         - Attack a dragon or squirrel        │
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
    ║                 [Squirrel Warren]                           ║
    ║                        │                                    ║
    ║                   [Acorn Grove]                             ║
    ║                        │                                    ║
    ║                   [Hollow Tree]                             ║
    ║                        │                                    ║
    ║                  [Treetop Canopy]                           ║
    ║                        │                                    ║
    ║             [Squirrel King's Chamber]                       ║
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
