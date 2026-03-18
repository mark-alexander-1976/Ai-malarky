# Full Spoiler Walkthrough

This guide contains the complete solution, including the winning route, key items, and the late-game sequence.

If you want only nudges, use the low-spoiler version instead:

- [GUIDE_HINTS.md](GUIDE_HINTS.md)

## Objective

Reach the Treasure Vault and take the Golden Crown.

Picking up the crown immediately wins the game.

## Route overview

The cleanest run has four phases:

1. Gather your utility items in the wilderness.
2. Use the river route to reach the castle.
3. Arm yourself and kill the dragon.
4. Push through the squirrel gauntlet and claim the crown.

## Phase 1: Gather the expedition kit

This route collects the items needed for a safe and reliable clear.

### Forest, mountain, and ruins

From the Forest Clearing:

1. `take lantern`
2. `north`
3. `north`
4. `take torch`
5. `north`
6. `take map`
7. `south`
8. `west`
9. `down`
10. `take amulet`
11. `up`
12. `east`
13. `south`
14. `east`
15. `take iron key`

Why this matters:

- the Lantern is required to descend safely into the cave branch
- the Amulet protects you from swamp damage
- the Iron Key opens the castle gates
- the Map is optional, but useful

### Village supplies

Continue with:

1. `south`
2. `east`
3. `south`
4. `west`
5. `take rope`
6. `east`
7. `north`
8. `take water flask`

Why this matters:

- the Rope is required to cross from the Riverbank to the Stone Bridge
- the Water Flask restores 20 health when used

## Phase 2: Reach the castle

From the Desert Oasis, return toward the Forest Clearing and use the western route:

1. `west`
2. `west`
3. `west`
4. `south`
5. `west`
6. `south`
7. `south`
8. `east`

Notes:

- The Amulet prevents the swamp from dealing 20 damage when you pass through it.
- If you want the extra feedback text, use `use rope` while standing at the Riverbank before going south.
- If you want the extra feedback text, use `use iron key` while standing at the Castle Gates before going east.
- Carrying the Rope and Iron Key is what matters for movement.

## Phase 3: Arm yourself and slay the dragon

### Castle preparation

Inside the castle:

1. `east`
2. `take sword`
3. `west`
4. `north`

Important:

- Entering the Dragon's Lair without the Sword kills you instantly.
- Once you have the Sword, the dragon fight becomes survivable.

### Dragon fight

In the Dragon's Lair:

1. `fight dragon`

Result:

- the dragon dies
- you lose 10 health
- the passage north opens

## Phase 4: Survive the squirrel gauntlet

After the dragon, keep moving north and defeat each squirrel encounter as it appears.

This section is slightly randomized. Sometimes a squirrel appears the moment you enter an area. Sometimes you only get a warning that something is watching you, and the squirrel does not fully reveal itself until you try to go north.

The reliable method is:

1. Enter the area.
2. If a squirrel is already challenging you, `fight squirrel`.
3. If no squirrel is visible yet, try `north` once to trigger the ambush.
4. `fight squirrel`.
5. Repeat until you can move north into the next area.

Area order:

1. Squirrel Warren
2. Acorn Grove
3. Hollow Tree
4. Treetop Canopy
5. Squirrel King's Chamber
6. Treasure Vault

### Expected damage with the Sword

With the Sword, the expected damage across these five fights is:

- Squirrel Warren: 5
- Acorn Grove: 5
- Hollow Tree: 5
- Treetop Canopy: 10
- Squirrel King's Chamber: 15

Total expected squirrel damage with the Sword: 40

Combined with the dragon fight, plan around losing about 50 health across the full endgame.

If needed, use the Water Flask before or during the final stretch.

### Final action

In the Treasure Vault:

1. `take golden crown`

That immediately triggers the win.

## Quick-reference checklists

### Minimal winning checklist

These items matter most:

- Lantern
- Rope
- Iron Key
- Sword

These are not strictly mandatory for the shortest possible win, but are strongly recommended:

- Amulet
- Water Flask
- Map

### Common failure points

- Trying to descend into the cave without the Lantern
- Crossing the swamp repeatedly without the Amulet
- Entering the Dragon's Lair without the Sword
- Reaching the squirrel sequence with low health
- Forgetting that taking the Golden Crown is the actual win trigger

## Reference appendix

### Where each item is found

- Lantern: Forest Clearing
- Torch: Mountain Trail
- Map: Summit Peak
- Amulet: Underground Lake
- Iron Key: Ancient Ruins
- Water Flask: Desert Oasis
- Rope: Old Tavern
- Gold Coin: Village Marketplace
- Sword: Castle Armory
- Golden Crown: Treasure Vault

### World map appendix

This mirrors the in-game map provided when you use or read the Map item.

```text
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
║                  [Treasure Vault]                           ║
╚═════════════════════════════════════════════════════════════╝
```

### Example winning command path

This is a full start-to-finish route for the non-random parts of the game. The squirrel gauntlet is not listed as a single exact command string because encounter timing varies from run to run.

```text
take lantern
north
north
take torch
north
take map
south
west
down
take amulet
up
east
south
east
take iron key
south
east
south
west
take rope
east
north
take water flask
west
west
west
south
south
south
east
east
take sword
west
north
fight dragon
take golden crown
```

Once the dragon is dead, follow the squirrel method described above until you reach the Treasure Vault, then `take golden crown`.