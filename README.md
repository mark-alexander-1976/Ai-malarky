# The Lost Kingdom

The Lost Kingdom is a C# text adventure about forgotten roads, ruined strongholds, bad decisions in the dark, and one very valuable crown.

You are an adventurer chasing the legend of the Golden Crown of the Ancient Kingdom. To claim it, you will need to cross wilderness, descend into darkness, unlock sealed paths, survive deadly encounters, and reach the Treasure Vault alive.

## What the game is

You begin in a forest clearing at the edge of a fallen realm. From there, the world opens into forests, mountain trails, ruins, caves, abandoned settlements, swamps, river crossings, castle grounds, and stranger territory beyond the dragon's lair.

Along the way you will:

- gather practical gear such as the Lantern, Rope, Iron Key, Sword, and Water Flask
- navigate dark areas where moving blindly is a fast way to die
- unlock blocked routes by carrying the right item at the right time
- manage your health through hazards, attrition, and combat
- defeat the dragon and press onward toward the Treasure Vault

The game uses a classic text parser with short commands and synonyms, so you can play with inputs like `north`, `n`, `go north`, `take lantern`, `use rope`, or `fight dragon`.

## How to run

This project targets .NET 10.

From the repository root:

```powershell
dotnet run --project .\TextAdventure\TextAdventure.csproj
```

## Command overview

Common commands include:

- `look` or `l`
- `examine <thing>` or `x <thing>`
- `go <direction>` or `n`, `s`, `e`, `w`, `u`, `d`
- `take <item>` and `drop <item>`
- `inventory` or `i`
- `use <item>`
- `read <item>`
- `fight <enemy>`
- `health` or `status`
- `help`
- `quit`

## Sample session

This is the opening of a real run, shown here so the command style and game presentation are clear before you start:

```text
╔══════════════════════════════════════════════════════════════╗
║          T H E   L O S T   K I N G D O M                     ║
║                A Text Adventure                              ║
╚══════════════════════════════════════════════════════════════╝

In the age before memory, a great kingdom fell.
Its legendary Golden Crown was lost in the chaos.
You are an adventurer seeking fame and fortune.
Find the Golden Crown and claim it as your own!

Type HELP for a list of commands.

Forest Clearing
───────────────
You stand in a sunlit clearing surrounded by ancient oaks. Rays of golden light
filter through the leaf canopy. A well-worn path leads north into darker forest.
Rocky ground rises to the east, a marshy smell drifts from the south, and the
sound of rushing water comes from the west.

You can see:
  - Lantern

Exits: NORTH, EAST, SOUTH, WEST

> take lantern
You take the Lantern.

> north
Dark Forest
───────────
It is pitch black. You cannot see a thing.
You could easily stumble into danger without a light source.
```

## Strategy guides

Two guides are included depending on how much you want spoiled:

- [GUIDE_HINTS.md](GUIDE_HINTS.md): low-spoiler strategy advice and progressive hints
- [GUIDE_WALKTHROUGH.md](GUIDE_WALKTHROUGH.md): full-spoiler step-by-step route to victory

## Notes for players

- Carry a light source early.
- Some paths cannot be used until you have the correct item.
- Some dangerous areas can be survived more safely with the right equipment.
- Picking up the Golden Crown ends the game with a win.
- If you want help without losing the sense of discovery, use the low-spoiler guide first.
