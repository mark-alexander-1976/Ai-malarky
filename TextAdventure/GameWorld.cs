namespace TextAdventure;

/// <summary>
/// Sets up and owns the 20 interlinked locations and all game items.
/// </summary>
public class GameWorld
{
    private readonly Dictionary<int, Location> _locations = [];

    public GameWorld()
    {
        BuildWorld();
    }

    public Location GetLocation(int id) => _locations[id];

    private void BuildWorld()
    {
        // ─────────────────────────────────────────────────────────────────
        // ITEMS
        // ─────────────────────────────────────────────────────────────────
        var lantern     = new Item { Id =  1, Name = "Lantern",      Description = "A brass lantern that burns brightly. It will light your way through the darkest places." };
        var torch       = new Item { Id =  2, Name = "Torch",        Description = "A pine torch, already lit. It gives a warm flickering light but won't last forever." };
        var rope        = new Item { Id =  3, Name = "Rope",         Description = "A coil of sturdy hemp rope. It looks strong enough to hold your weight." };
        var ironKey     = new Item { Id =  4, Name = "Iron Key",     Description = "A heavy iron key, green with age. One side bears the crest of an eagle." };
        var amulet      = new Item { Id =  5, Name = "Amulet",       Description = "A jade amulet carved into the shape of a serpent. It radiates a faint protective warmth." };
        var sword       = new Item { Id =  6, Name = "Sword",        Description = "A well-balanced longsword. The blade is still keen despite its age." };
        var map         = new Item { Id =  7, Name = "Map",          Description = "A crinkled parchment map of the surrounding lands. Useful for getting your bearings." };
        var waterFlask  = new Item { Id =  8, Name = "Water Flask",  Description = "A leather flask filled with cool, clear water. Vital for crossing the desert regions." };
        var goldCoin    = new Item { Id =  9, Name = "Gold Coin",    Description = "A large gold coin stamped with the face of a long-dead king. Clearly valuable." };
        var crown       = new Item { Id = 10, Name = "Golden Crown", Description = "The fabled Golden Crown of the Ancient Kingdom! Encrusted with precious gems, it gleams magnificently." };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 1 – Forest Clearing (starting location)
        // ─────────────────────────────────────────────────────────────────
        _locations[1] = new Location
        {
            Id = 1,
            Name = "Forest Clearing",
            Description = "You stand in a sunlit clearing surrounded by ancient oaks. " +
                          "Rays of golden light filter through the leaf canopy. " +
                          "A well-worn path leads north into darker forest. " +
                          "Rocky ground rises to the east, a marshy smell drifts from the south, " +
                          "and the sound of rushing water comes from the west.",
            Exits = { ["north"] = 2, ["east"] = 3, ["south"] = 13, ["west"] = 14 },
            Items = { lantern },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 2 – Dark Forest
        // ─────────────────────────────────────────────────────────────────
        _locations[2] = new Location
        {
            Id = 2,
            Name = "Dark Forest",
            Description = "A dense, forbidding forest closes in on all sides. " +
                          "Gnarled branches interlock overhead, shutting out almost all light. " +
                          "Strange rustlings come from the undergrowth. " +
                          "The trail continues north toward the mountains, south leads back to the clearing, " +
                          "and crumbling stonework is just visible to the east.",
            IsDark = true,
            Exits = { ["south"] = 1, ["north"] = 4, ["east"] = 8 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 3 – Rocky Path
        // ─────────────────────────────────────────────────────────────────
        _locations[3] = new Location
        {
            Id = 3,
            Name = "Rocky Path",
            Description = "A rough track picks its way between boulders and thorny scrub. " +
                          "The ground is dry and cracked. Ancient ruins are visible to the north " +
                          "and an expanse of shimmering sand lies to the east. " +
                          "The forest clearing is back to the west.",
            Exits = { ["west"] = 1, ["north"] = 8, ["east"] = 9 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 4 – Mountain Trail
        // ─────────────────────────────────────────────────────────────────
        _locations[4] = new Location
        {
            Id = 4,
            Name = "Mountain Trail",
            Description = "A winding trail climbs steeply up the mountainside. " +
                          "Loose scree makes every step treacherous. " +
                          "The path continues upward to the north and the dark forest lies to the south. " +
                          "A dark cave mouth gapes in the cliff face to the west.",
            Exits = { ["south"] = 2, ["north"] = 5, ["west"] = 6 },
            Items = { torch },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 5 – Summit Peak
        // ─────────────────────────────────────────────────────────────────
        _locations[5] = new Location
        {
            Id = 5,
            Name = "Summit Peak",
            Description = "You stand atop the mountain, buffeted by cold winds. " +
                          "The entire land is spread out below you in breathtaking panorama: " +
                          "the dark forest, the distant castle, the glittering river and desert beyond. " +
                          "A rolled parchment is wedged in a rock crevice.",
            Exits = { ["south"] = 4 },
            Items = { map },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 6 – Hidden Cave
        // ─────────────────────────────────────────────────────────────────
        _locations[6] = new Location
        {
            Id = 6,
            Name = "Hidden Cave",
            Description = "You are inside a cave in the mountainside. " +
                          "Stalactites drip cold water from the ceiling. " +
                          "The cave entrance opens to the east. " +
                          "A narrow passage leads downward into the depths below.",
            IsDark = true,
            Exits = { ["east"] = 4 },
            LockedExits = { ["down"] = "Lantern" },    // needs light to descend safely
        };
        // We must also add the down exit unconditionally – the lock check is handled in Game.cs
        _locations[6].Exits["down"] = 7;

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 7 – Underground Lake
        // ─────────────────────────────────────────────────────────────────
        _locations[7] = new Location
        {
            Id = 7,
            Name = "Underground Lake",
            Description = "An eerie underground lake stretches before you. " +
                          "Its utterly still black surface reflects your light like a dark mirror. " +
                          "Blind fish ripple the surface occasionally. " +
                          "The passage back up is to the north.",
            IsDark = true,
            Exits = { ["up"] = 6 },
            Items = { amulet },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 8 – Ancient Ruins
        // ─────────────────────────────────────────────────────────────────
        _locations[8] = new Location
        {
            Id = 8,
            Name = "Ancient Ruins",
            Description = "The crumbling ruins of a long-forgotten temple. " +
                          "Moss-covered stone columns have toppled, and carved reliefs on the walls " +
                          "depict strange ceremonies. Something glints among the rubble. " +
                          "Paths lead west into the dark forest and south along the rocky track.",
            Exits = { ["west"] = 2, ["south"] = 3 },
            Items = { ironKey },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 9 – Desert Oasis
        // ─────────────────────────────────────────────────────────────────
        _locations[9] = new Location
        {
            Id = 9,
            Name = "Desert Oasis",
            Description = "A blessed oasis of palm trees and cool water in the middle of a scorched desert. " +
                          "The air is heavy with the scent of flowers. A leather flask sits beside the well. " +
                          "The rocky path is back to the west; a dusty road leads south toward a village.",
            Exits = { ["west"] = 3, ["south"] = 10 },
            Items = { waterFlask },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 10 – Abandoned Village
        // ─────────────────────────────────────────────────────────────────
        _locations[10] = new Location
        {
            Id = 10,
            Name = "Abandoned Village",
            Description = "A village of tumbledown cottages, long since deserted. " +
                          "Weeds push through the cobblestones; shutters bang in the breeze. " +
                          "The desert road stretches north. An old marketplace lies to the east " +
                          "and a weathered tavern sign creaks to the west.",
            Exits = { ["north"] = 9, ["east"] = 11, ["west"] = 12 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 11 – Village Marketplace
        // ─────────────────────────────────────────────────────────────────
        _locations[11] = new Location
        {
            Id = 11,
            Name = "Village Marketplace",
            Description = "A silent marketplace. Stalls stand empty, their canopies in tatters. " +
                          "Dried herbs and rotted produce litter the ground. " +
                          "Beneath an overturned barrel a gold coin catches the light. " +
                          "The village square is back to the west.",
            Exits = { ["west"] = 10 },
            Items = { goldCoin },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 12 – Old Tavern
        // ─────────────────────────────────────────────────────────────────
        _locations[12] = new Location
        {
            Id = 12,
            Name = "Old Tavern",
            Description = "The common room of a long-abandoned tavern. " +
                          "Dust covers every surface; empty tankards and bottles line the bar. " +
                          "A coil of rope hangs on a peg by the door – useful perhaps? " +
                          "The village square is back to the east.",
            Exits = { ["east"] = 10 },
            Items = { rope },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 13 – Muddy Swamp
        // ─────────────────────────────────────────────────────────────────
        _locations[13] = new Location
        {
            Id = 13,
            Name = "Muddy Swamp",
            Description = "A fetid, treacherous swamp. " +
                          "Dark, oily water fills the gaps between tussocks of pale grass. " +
                          "The air reeks of sulphur and decay; clouds of biting insects surround you. " +
                          "With care you can move north back to the clearing or east toward the river.",
            IsDangerous = true,
            SafeItemName = "Amulet",
            Exits = { ["north"] = 1, ["east"] = 14 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 14 – Riverbank
        // ─────────────────────────────────────────────────────────────────
        _locations[14] = new Location
        {
            Id = 14,
            Name = "Riverbank",
            Description = "You stand on the muddy bank of a swift, cold river. " +
                          "The current looks dangerous to swim. Old stone pillars on both banks " +
                          "hint at a bridge long since washed away. " +
                          "The forest clearing is to the east, the swamp lies west, " +
                          "and the ruined bridge foundations are to the south – if only you had a rope.",
            Exits = { ["east"] = 1, ["west"] = 13 },
            LockedExits = { ["south"] = "Rope" },
        };
        _locations[14].Exits["south"] = 15;

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 15 – Stone Bridge
        // ─────────────────────────────────────────────────────────────────
        _locations[15] = new Location
        {
            Id = 15,
            Name = "Stone Bridge",
            Description = "You stand on the remnants of an ancient stone bridge, " +
                          "your rope lashed to the crumbling parapets to help you across. " +
                          "The river churns far below. " +
                          "The riverbank lies to the north and a forbidding castle gateway waits to the south.",
            Exits = { ["north"] = 14, ["south"] = 16 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 16 – Castle Gates
        // ─────────────────────────────────────────────────────────────────
        _locations[16] = new Location
        {
            Id = 16,
            Name = "Castle Gates",
            Description = "Massive iron-bound oak gates bar the entrance to the castle. " +
                          "A heavy iron lock secures the gate bar. " +
                          "The castle walls are high and smooth – there is no other way in. " +
                          "The bridge is back to the north.",
            Exits = { ["north"] = 15 },
            LockedExits = { ["east"] = "Iron Key" },
        };
        _locations[16].Exits["east"] = 17;

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 17 – Castle Courtyard
        // ─────────────────────────────────────────────────────────────────
        _locations[17] = new Location
        {
            Id = 17,
            Name = "Castle Courtyard",
            Description = "A wide cobbled courtyard inside the castle walls. " +
                          "Crumbling statues of armoured knights line the walls. " +
                          "An ominous darkness pours from a great archway to the north. " +
                          "An iron-studded door leads east to what must be the armory, " +
                          "and the castle gates are to the west.",
            Exits = { ["west"] = 16, ["north"] = 19, ["east"] = 18 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 18 – Castle Armory
        // ─────────────────────────────────────────────────────────────────
        _locations[18] = new Location
        {
            Id = 18,
            Name = "Castle Armory",
            Description = "The castle's old armoury. Racks of rusted blades and corroded armour " +
                          "line every wall. But one sword stands apart from the rest: " +
                          "its blade is bright, its edge still keen. It seems to be waiting for you. " +
                          "The courtyard is back to the west.",
            Exits = { ["west"] = 17 },
            Items = { sword },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 19 – Dragon's Lair
        // ─────────────────────────────────────────────────────────────────
        _locations[19] = new Location
        {
            Id = 19,
            Name = "Dragon's Lair",
            Description = "A vast cavern beneath the castle, hot and reeking of sulphur. " +
                          "The bones of countless adventurers litter the scorched floor. " +
                          "A HUGE RED DRAGON coils in the shadows, its ember eyes fixed upon you. " +
                          "The courtyard is back to the south. Beyond the dragon, a vault lies to the north.",
            IsDangerous = true,
            SafeItemName = "Sword",
            Exits = { ["south"] = 17 },
            // north exit to vault only opens after dragon is defeated – handled in Game.cs
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 21 – Squirrel Warren
        // ─────────────────────────────────────────────────────────────────
        _locations[21] = new Location
        {
            Id = 21,
            Name = "Squirrel Warren",
            Description = "A cramped tunnel of roots and packed earth stretches ahead, lit by shafts of dusty light. " +
                          "Nut shells crunch underfoot, and every shadow seems to twitch just out of sight. " +
                          "The dragon's lair lies back to the south and a narrow burrow slopes north.",
            Exits = { ["south"] = 19, ["north"] = 22 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 22 – Acorn Grove
        // ─────────────────────────────────────────────────────────────────
        _locations[22] = new Location
        {
            Id = 22,
            Name = "Acorn Grove",
            Description = "You emerge into a circular grove of ancient oaks where the ground is carpeted with acorns. " +
                          "Tiny claw marks score the bark at every height, and the branches sway although no wind blows. " +
                          "The warren lies south; a darker path continues north.",
            Exits = { ["south"] = 21, ["north"] = 23 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 23 – Hollow Tree
        // ─────────────────────────────────────────────────────────────────
        _locations[23] = new Location
        {
            Id = 23,
            Name = "Hollow Tree",
            Description = "Inside the trunk of an impossibly large oak, spiraling ridges of wood rise like steps around you. " +
                          "The air smells of sap, bark, and menace. Something chatters high above in the dark. " +
                          "The grove is south and a split in the trunk leads north.",
            Exits = { ["south"] = 22, ["north"] = 24 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 24 – Treetop Canopy
        // ─────────────────────────────────────────────────────────────────
        _locations[24] = new Location
        {
            Id = 24,
            Name = "Treetop Canopy",
            Description = "You pick your way across woven branches high above the forest floor. " +
                          "Leaves whisper around you, and discarded pine cones lie in careful little piles like ammunition stores. " +
                          "The hollow tree descends to the south; a rope-vine bridge stretches north.",
            Exits = { ["south"] = 23, ["north"] = 25 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 25 – Squirrel King's Chamber
        // ─────────────────────────────────────────────────────────────────
        _locations[25] = new Location
        {
            Id = 25,
            Name = "Squirrel King's Chamber",
            Description = "A vaulted chamber of bent branches and polished acorns opens before you. " +
                          "At its centre stands a ludicrously tiny throne built from twigs, amber, and bottle caps. " +
                          "The canopy is back to the south, while an arch of interlocked tails leads north toward the vault.",
            Exits = { ["south"] = 24, ["north"] = 20 },
        };

        // ─────────────────────────────────────────────────────────────────
        // LOCATION 20 – Treasure Vault
        // ─────────────────────────────────────────────────────────────────
        _locations[20] = new Location
        {
            Id = 20,
            Name = "Treasure Vault",
            Description = "A magnificent vault blazing with gold and jewels. " +
                          "Piles of ancient coins, gem-encrusted goblets and gilded armour " +
                          "fill every corner. In the very centre, on a velvet cushion, " +
                          "sits the legendary GOLDEN CROWN of the Ancient Kingdom.",
            Exits = { ["south"] = 25 },
            Items = { crown },
        };
    }
}
