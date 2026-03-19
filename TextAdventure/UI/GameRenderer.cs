namespace TextAdventure.UI;

using TextAdventure.Models;

public class GameRenderer
{
    public void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("""
        ╔══════════════════════════════════════════════╗
        ║     ☆  ISLAND OF TREASURES  ☆               ║
        ║     A Text Adventure                         ║
        ║     Inspired by Adventureland (1978)         ║
        ╚══════════════════════════════════════════════╝
        """);
        Console.ResetColor();
        Console.WriteLine("Type two-word commands like GO NORTH, GET LAMP, USE KEY.");
        Console.WriteLine("Other commands: LOOK, INVENTORY, SCORE, HELP, QUIT\n");
    }

    public string? ReadCommand()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\n> ");
        Console.ResetColor();
        return Console.ReadLine();
    }

    public void Print(string text) => Console.WriteLine(text);

    public void PrintRoomHeader(string name)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n── {name} ──");
        Console.ResetColor();
    }

    public void PrintItem(Item item)
    {
        Console.ForegroundColor = item.IsTreasure ? ConsoleColor.Yellow : ConsoleColor.White;
        Console.WriteLine($"  - {item.Description}");
        Console.ResetColor();
    }

    public void PrintVictory(int moves)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("""

        ╔══════════════════════════════════════════════╗
        ║  ☆ ☆ ☆  CONGRATULATIONS!  ☆ ☆ ☆            ║
        ║                                              ║
        ║  You've collected ALL the treasures and      ║
        ║  returned them to the sunny meadow!          ║
        ║                                              ║
        ║  You are a TRUE ADVENTURER!                  ║
        ╚══════════════════════════════════════════════╝
        """);
        Console.ResetColor();
        Console.WriteLine($"You completed the game in {moves} moves.");
    }

    public void PrintHelp()
    {
        Console.WriteLine("""
        ┌─────────────────────────────────────────────┐
        │ COMMANDS:                                    │
        │   GO <dir>  - Move (NORTH/SOUTH/EAST/WEST/  │
        │               UP/DOWN or N/S/E/W/U/D)       │
        │   GET <item> - Pick up an item (or GET ALL)  │
        │   DROP <item> - Drop an item                 │
        │   USE <item> - Use an item                   │
        │   EXAMINE <item> - Look closer at something  │
        │   LIGHT LAMP - Light the lamp (need matches) │
        │   CUT <thing> - Cut something (need axe)     │
        │   SPRAY <thing> - Spray repellent             │
        │   READ <item> - Read text on something       │
        │   LOOK       - Look around (or L)            │
        │   INVENTORY  - Check your pack (or I)        │
        │   SCORE      - Check your score              │
        │   QUIT       - End the game                  │
        │                                              │
        │ GOAL: Find all 5 treasures (*starred*) and   │
        │       bring them to the Sunny Meadow!        │
        └─────────────────────────────────────────────┘
        """);
    }
}
