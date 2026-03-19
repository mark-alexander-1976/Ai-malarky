using TextAdventure.UI;

namespace TextAdventure.Engine;

public class CommandHandler(GameState state, GameRenderer renderer)
{
    public void Execute(ParsedCommand command)
    {
        switch (command.Verb)
        {
            case "GO":                          DoGo(command.Noun); break;
            case "GET" or "TAKE" or "GRAB":     DoGet(command.Noun); break;
            case "DROP" or "PUT" or "LEAVE":    DoDrop(command.Noun); break;
            case "USE":                         DoUse(command.Noun); break;
            case "LIGHT":                       DoLight(command.Noun); break;
            case "CUT":                         DoCut(command.Noun); break;
            case "LOOK" or "L":                 Look(); break;
            case "INVENTORY" or "I" or "INV":   DoInventory(); break;
            case "EXAMINE" or "X" or "EX":      DoExamine(command.Noun); break;
            case "SCORE":                       DoScore(); break;
            case "HELP" or "?":                 renderer.PrintHelp(); break;
            case "QUIT" or "Q" or "EXIT":
                state.GameOver = true;
                renderer.Print("Thanks for playing!");
                break;
            case "SPRAY":                       DoSpray(command.Noun); break;
            case "FEED":                        DoFeed(command.Noun); break;
            case "READ":                        DoRead(command.Noun); break;
            default:
                renderer.Print("I don't understand that. Type HELP for commands.");
                break;
        }
    }

    // ── Look ────────────────────────────────────────────────────────────────

    public void Look()
    {
        renderer.PrintRoomHeader(state.CurrentRoom.Name);

        if (!state.CanSee)
        {
            renderer.Print("It's pitch dark! You can't see a thing.");
            return;
        }

        renderer.Print(state.CurrentRoom.Description);

        var items = state.ItemsHere.ToList();
        if (items.Count > 0)
        {
            renderer.Print("\nYou can see:");
            foreach (var item in items)
                renderer.PrintItem(item);
        }

        var exits = state.CurrentRoom.Exits.Select(e => e.Direction.ToUpper());
        renderer.Print($"\nExits: {string.Join(", ", exits)}");
    }

    // ── Movement ────────────────────────────────────────────────────────────

    private void DoGo(string direction)
    {
        if (string.IsNullOrEmpty(direction))
        {
            renderer.Print("Go where? Try: GO NORTH, GO SOUTH, etc.");
            return;
        }

        var exit = state.CurrentRoom.Exits.FirstOrDefault(
            e => e.Direction.Equals(direction, StringComparison.OrdinalIgnoreCase));

        if (exit is null)
        {
            renderer.Print("You can't go that way.");
            return;
        }

        if (exit.RequiredItem is not null && !state.HasItem(exit.RequiredItem))
        {
            renderer.Print(exit.BlockedMessage ?? "Something blocks your way.");
            return;
        }

        // Dragon's Lair special encounter
        if (exit.RoomId == 9 && !state.DragonAsleep)
        {
            if (state.HasItem("repellent"))
            {
                renderer.Print("A massive dragon blocks the cave! But you have the repellent...");
                renderer.Print("Type SPRAY DRAGON to use it, or retreat WEST.");
                state.CurrentRoomId = exit.RoomId;
                state.Moves++;
                return;
            }
            renderer.Print("A massive dragon blocks the cave entrance! It breathes fire!\nYou barely escape with your life! You need some kind of protection...");
            return;
        }

        state.CurrentRoomId = exit.RoomId;
        state.Moves++;

        if (state.CurrentRoom.IsDark && !state.CanSee)
        {
            renderer.PrintRoomHeader(state.CurrentRoom.Name);
            renderer.Print("It's pitch dark! You can't see a thing.");
            renderer.Print("You might fall into a pit! You should find a light source.");
            return;
        }

        Look();
    }

    // ── Item manipulation ───────────────────────────────────────────────────

    private void DoGet(string noun)
    {
        if (string.IsNullOrEmpty(noun)) { renderer.Print("Get what?"); return; }

        if (noun == "ALL")
        {
            var here = state.ItemsHere.ToList();
            if (here.Count == 0) { renderer.Print("There's nothing here to take."); return; }
            foreach (var item in here)
            {
                item.RoomId = GameState.InventoryLocation;
                renderer.Print($"Taken: {item.Name}");
            }
            return;
        }

        if (!state.CanSee) { renderer.Print("It's too dark to see anything!"); return; }

        var it = state.FindItem(noun, state.CurrentRoomId);
        if (it is null) { renderer.Print("I don't see that here."); return; }
        if (state.PlayerItems.Count() >= GameState.MaxInventory)
        {
            renderer.Print("You're carrying too much! Drop something first.");
            return;
        }

        it.RoomId = GameState.InventoryLocation;
        renderer.Print($"OK, you took the {it.Name}.");
    }

    private void DoDrop(string noun)
    {
        if (string.IsNullOrEmpty(noun)) { renderer.Print("Drop what?"); return; }

        var it = state.FindItem(noun, GameState.InventoryLocation);
        if (it is null) { renderer.Print("You're not carrying that."); return; }

        it.RoomId = state.CurrentRoomId;
        renderer.Print($"OK, you dropped the {it.Name}.");

        if (state.CurrentRoomId == GameState.TreasureRoom
            && state.TreasuresScored == state.TreasureCount)
        {
            renderer.PrintVictory(state.Moves);
            DoScore();
            state.GameOver = true;
        }
    }

    // ── Item usage ──────────────────────────────────────────────────────────

    private void DoUse(string noun)
    {
        if (string.IsNullOrEmpty(noun)) { renderer.Print("Use what?"); return; }

        switch (noun.ToUpperInvariant())
        {
            case "LAMP":
            case "MATCHES" or "MATCH":
                DoLight("LAMP");
                break;
            case "KEY":
                if (!state.HasItem("key")) { renderer.Print("You don't have a key."); return; }
                if (state.CurrentRoomId == 5)
                    renderer.Print("You insert the iron key into the lock and turn it.\nThe heavy door swings open with a groan!");
                else
                    renderer.Print("There's nothing to use the key on here.");
                break;
            case "ROPE":
                if (!state.HasItem("rope")) { renderer.Print("You don't have a rope."); return; }
                if (state.CurrentRoomId == 7)
                    renderer.Print("You could just GO SOUTH - the rope in your pack gives you confidence to cross!");
                else
                    renderer.Print("There's no good place to use the rope here.");
                break;
            case "AXE":
                DoCut("VINES");
                break;
            case "REPELLENT" or "BOTTLE" or "DRAGON-B-GONE":
                DoSpray("DRAGON");
                break;
            default:
                renderer.Print($"I'm not sure how to use the {noun.ToLower()}.");
                break;
        }
    }

    private void DoLight(string noun)
    {
        if (noun.Equals("LAMP", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(noun))
        {
            if (!state.HasItem("lamp"))    { renderer.Print("You don't have a lamp."); return; }
            if (!state.HasItem("matches")) { renderer.Print("You have no way to light the lamp."); return; }
            if (state.LampLit)             { renderer.Print("The lamp is already lit."); return; }

            state.LampLit = true;
            renderer.Print("The lamp flickers to life, casting a warm golden glow!");

            if (state.CurrentRoom.IsDark)
                Look();
        }
        else
        {
            renderer.Print("You can't light that.");
        }
    }

    private void DoCut(string noun)
    {
        if (!state.HasItem("axe"))
        {
            renderer.Print("You don't have anything to cut with.");
            return;
        }

        if (noun.Equals("VINES", StringComparison.OrdinalIgnoreCase)
            || noun.Equals("VINE", StringComparison.OrdinalIgnoreCase))
        {
            if (state.CurrentRoomId == 0)
                renderer.Print("You hack through the thick vines with your axe!\nThe passage east is now clear.");
            else
                renderer.Print("There are no vines to cut here.");
        }
        else
        {
            renderer.Print($"You can't cut the {noun.ToLower()}.");
        }
    }

    private void DoSpray(string noun)
    {
        if (!state.HasItem("repellent"))
        {
            renderer.Print("You don't have any repellent.");
            return;
        }

        if (noun.Equals("DRAGON", StringComparison.OrdinalIgnoreCase) && state.CurrentRoomId == 9)
        {
            state.DragonAsleep = true;
            var repellent = state.FindItem("repellent", GameState.InventoryLocation)!;
            repellent.RoomId = GameState.NowhereLocation;
            renderer.Print("You spray the DRAGON-B-GONE at the massive beast!\nThe dragon sneezes, yawns, and slumps to the ground... fast asleep!\nThe way is now safe.");
            Look();
        }
        else if (state.CurrentRoomId != 9)
        {
            renderer.Print("There's nothing to spray here.");
        }
        else
        {
            renderer.Print("Spray it at what?");
        }
    }

    private void DoFeed(string noun)
    {
        if (noun.Equals("DRAGON", StringComparison.OrdinalIgnoreCase) && state.CurrentRoomId == 9)
        {
            if (state.HasItem("bread"))
                renderer.Print("You toss the bread at the dragon. It incinerates mid-air.\nThe dragon looks unimpressed. Maybe try something else...");
            else
                renderer.Print("You have nothing to feed it.");
        }
        else
        {
            renderer.Print("Feed what?");
        }
    }

    private void DoRead(string noun)
    {
        if (noun.Equals("SCROLL", StringComparison.OrdinalIgnoreCase)
            || noun.Equals("ANCIENT", StringComparison.OrdinalIgnoreCase))
        {
            if (state.HasItem("scroll") || state.IsHere("scroll"))
                renderer.Print("The scroll reads:\n\"Five treasures hidden across the isle shall bring glory\n to the one who returns them to where the sun shines brightest.\"");
            else
                renderer.Print("You don't see a scroll.");
        }
        else if (noun.Equals("SIGN", StringComparison.OrdinalIgnoreCase))
        {
            if (state.CurrentRoomId == 0)
                renderer.Print("The sign reads: \"BRING TREASURES HERE FOR GLORY!\"");
            else
                renderer.Print("There's no sign here.");
        }
        else
        {
            renderer.Print("You can't read that.");
        }
    }

    // ── Examine / Inventory / Score ─────────────────────────────────────────

    private void DoExamine(string noun)
    {
        if (string.IsNullOrEmpty(noun)) { renderer.Print("Examine what?"); return; }

        var item = state.FindItemAnywhere(noun);
        if (item is not null)
        {
            renderer.Print(item.Description);
            return;
        }

        switch (noun.ToUpperInvariant())
        {
            case "TREE" or "OAK":
                if (state.CurrentRoomId == 2)
                    renderer.Print("The oak is enormous and ancient. Inside the hollow, something glints.");
                else
                    renderer.Print("I don't see that here.");
                break;
            case "DRAGON":
                if (state.CurrentRoomId == 9)
                    renderer.Print(state.DragonAsleep
                        ? "The dragon is fast asleep, snoring loudly. Wisps of smoke curl from its nostrils."
                        : "A massive red dragon! It eyes you hungrily. Its scales are like shields.");
                else
                    renderer.Print("I don't see a dragon here.");
                break;
            case "BRIDGE":
                if (state.CurrentRoomId == 7)
                    renderer.Print("A rickety rope bridge over a deep chasm. Some planks are missing.");
                else
                    renderer.Print("I don't see that here.");
                break;
            case "CRYSTALS" or "CRYSTAL":
                if (state.CurrentRoomId == 6)
                    renderer.Print("Beautiful crystals of every colour. They seem to pulse with inner light.");
                else
                    renderer.Print("I don't see that here.");
                break;
            default:
                renderer.Print("I don't see anything special about that.");
                break;
        }
    }

    private void DoInventory()
    {
        var items = state.PlayerItems.ToList();
        if (items.Count == 0)
        {
            renderer.Print("You're not carrying anything.");
            return;
        }

        renderer.Print("You are carrying:");
        foreach (var item in items)
            renderer.PrintItem(item);

        if (state.LampLit)
            renderer.Print("  (The lamp is lit)");
    }

    private void DoScore()
    {
        var scored = state.TreasuresScored;
        var total  = state.TreasureCount;
        renderer.Print($"\nTreasures stored: {scored} of {total}");
        renderer.Print($"Moves taken: {state.Moves}");

        if (scored == total)
            renderer.Print("*** MAXIMUM SCORE! ***");
        else if (scored > 0)
            renderer.Print("Keep searching! More treasures await.");
        else
            renderer.Print("Find treasures and bring them back to the sunny meadow!");
    }
}
