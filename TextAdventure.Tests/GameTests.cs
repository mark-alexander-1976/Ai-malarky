using Xunit;

public sealed class GameTests
{
    [Fact]
    public void Parse_MapsVintageVerbsAndPreservesSlotNames()
    {
        var save = Game.ParseForTesting("save Chapter One");
        var ascend = Game.ParseForTesting("ascend tower");
        var unlock = Game.ParseForTesting("unlock moon door");
        var listen = Game.ParseForTesting("listen");
        var inspect = Game.ParseForTesting("inspect shelves");
        var pull = Game.ParseForTesting("pull rope");
        var list = Game.ParseForTesting("list saves");
        var delete = Game.ParseForTesting("delete save Chapter One");
        var confirmDelete = Game.ParseForTesting("confirm delete");
        var rename = Game.ParseForTesting("rename save old to chapter one");
        var helpSaves = Game.ParseForTesting("help saves");
        var fight = Game.ParseForTesting("fight squirrel");
        var showMap = Game.ParseForTesting("show map");
        var map = Game.ParseForTesting("map");
        var setMiniMap = Game.ParseForTesting("set minimap on");
        var miniMap = Game.ParseForTesting("minimap off");

        Assert.Equal("SAVE", save.Verb);
        Assert.Equal("Chapter One", save.Noun);
        Assert.Equal("CLIMB", ascend.Verb);
        Assert.Equal("tower", ascend.Noun);
        Assert.Equal("OPEN", unlock.Verb);
        Assert.Equal("moon door", unlock.Noun);
        Assert.Equal("LISTEN", listen.Verb);
        Assert.Equal("SEARCH", inspect.Verb);
        Assert.Equal("PULL", pull.Verb);
        Assert.Equal("LISTSAVES", list.Verb);
        Assert.Null(list.Noun);
        Assert.Equal("DELETE", delete.Verb);
        Assert.Equal("save Chapter One", delete.Noun);
        Assert.Equal("CONFIRMDELETE", confirmDelete.Verb);
        Assert.Equal("delete", confirmDelete.Noun);
        Assert.Equal("RENAMESAVE", rename.Verb);
        Assert.Equal("save old to chapter one", rename.Noun);
        Assert.Equal("HELP", helpSaves.Verb);
        Assert.Equal("saves", helpSaves.Noun);
        Assert.Equal("ATTACK", fight.Verb);
        Assert.Equal("squirrel", fight.Noun);
        Assert.Equal("SHOWMAP", showMap.Verb);
        Assert.Null(showMap.Noun);
        Assert.Equal("SHOWMAP", map.Verb);
        Assert.Null(map.Noun);
        Assert.Equal("SETMINIMAP", setMiniMap.Verb);
        Assert.Equal("on", setMiniMap.Noun);
        Assert.Equal("SETMINIMAP", miniMap.Verb);
        Assert.Equal("off", miniMap.Noun);
    }

    [Fact]
    public void ShowMap_DisplaysCurrentLocation()
    {
        var game = new Game(saveDirectory: null, random: new Random(0));
        game.CurrentState.CurrentRoomId = "rope-chasm";

        var output = CaptureConsole(() => game.ExecuteForTesting("show map"));

        Assert.Contains("MAP OF THE VALLEY", output);
        Assert.Contains("[*Rope Chasm*", output);
        Assert.Contains("[Village Green", output);
        Assert.Contains("You are at: Rope Chasm.", output);
    }

    [Fact]
    public void MiniMap_CanBeEnabled_AndAppearsAfterMovement()
    {
        var game = new Game(saveDirectory: null, random: new Random(0));

        var output = CaptureConsole(() =>
        {
            game.ExecuteForTesting("set minimap on");
            game.ExecuteForTesting("go north");
        });

        Assert.True(game.CurrentState.AutoMiniMapEnabled);
        Assert.Contains("Mini-map display is now ON.", output);
        Assert.Contains("MINI-MAP", output);
        Assert.Contains("[*North Road*", output);
    }

    [Fact]
    public void World_HasTwentyRoomsAndValidExits()
    {
        var world = WorldFactory.CreateWorld();

        world.Validate();

        Assert.Equal(20, world.Rooms.Count);
        Assert.All(world.Rooms.Values, room => Assert.NotEmpty(room.Exits));
    }

    [Fact]
    public void SaveLoad_RoundTripsAcrossNamedSlots()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"textadventure-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(saveDirectory);

        try
        {
            var game = new Game(saveDirectory);
            var state = game.CurrentState;

            state.CurrentRoomId = "wizard-study";
            state.Moves = 17;
            state.Inventory.Add("lamp");
            state.Inventory.Add("silver-key");
            state.Flags.Add("lamp-lit");
            state.Flags.Add("vault-unlocked");
            state.SecuredTreasures.Add("opal-seal");
            state.AutoMiniMapEnabled = true;
            state.SquirrelsDefeated = 2;
            state.ActiveSquirrelLevel = 3;
            state.World.Rooms["lantern-hut"].ItemIds.Clear();

            game.SaveGameForTesting("Chapter One");

            state.CurrentRoomId = "moon-tower";
            state.Moves = 31;
            state.Inventory.Add("jeweled-crown");
            game.SaveGameForTesting("Chapter Two");

            Assert.True(File.Exists(game.GetSavePathForTesting("Chapter One")));
            Assert.True(File.Exists(game.GetSavePathForTesting("Chapter Two")));

            state.CurrentRoomId = "village-green";
            state.Inventory.Clear();
            state.Flags.Clear();
            state.SecuredTreasures.Clear();
            state.World.Rooms["lantern-hut"].ItemIds.Add("lamp");

            game.LoadGameForTesting("Chapter One");

            Assert.Equal("wizard-study", game.CurrentState.CurrentRoomId);
            Assert.Equal(17, game.CurrentState.Moves);
            Assert.Contains("lamp", game.CurrentState.Inventory);
            Assert.Contains("silver-key", game.CurrentState.Inventory);
            Assert.Contains("lamp-lit", game.CurrentState.Flags);
            Assert.Contains("vault-unlocked", game.CurrentState.Flags);
            Assert.Contains("opal-seal", game.CurrentState.SecuredTreasures);
            Assert.True(game.CurrentState.AutoMiniMapEnabled);
            Assert.Equal(2, game.CurrentState.SquirrelsDefeated);
            Assert.Equal(3, game.CurrentState.ActiveSquirrelLevel);
            Assert.DoesNotContain("lamp", game.CurrentState.World.Rooms["lantern-hut"].ItemIds);

            game.LoadGameForTesting("Chapter Two");

            Assert.Equal("moon-tower", game.CurrentState.CurrentRoomId);
            Assert.Equal(31, game.CurrentState.Moves);
            Assert.Contains("jeweled-crown", game.CurrentState.Inventory);
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public void SaveSlots_CanBeListedAndDeleted()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"textadventure-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(saveDirectory);

        try
        {
            var game = new Game(saveDirectory);

            game.SaveGameForTesting(null);
            game.SaveGameForTesting("Chapter One");
            game.SaveGameForTesting("Chapter Two");

            var listedSlots = game.ListSaveSlotsForTesting();

            Assert.Equal(new[] { "chapter-one", "chapter-two", "default" }, listedSlots.OrderBy(slot => slot).ToArray());

            game.DeleteSaveForTesting("save Chapter One");

            var listedAfterDelete = game.ListSaveSlotsForTesting();
            Assert.DoesNotContain("chapter-one", listedAfterDelete);
            Assert.Contains("chapter-two", listedAfterDelete);
            Assert.Contains("default", listedAfterDelete);
            Assert.False(File.Exists(game.GetSavePathForTesting("Chapter One")));
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public void DefaultSlot_RequiresConfirmDelete_AndSlotsCanBeRenamed()
    {
        var saveDirectory = Path.Combine(Path.GetTempPath(), $"textadventure-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(saveDirectory);

        try
        {
            var game = new Game(saveDirectory);

            game.SaveGameForTesting(null);
            game.SaveGameForTesting("old");

            game.DeleteSaveForTesting(null);

            Assert.True(File.Exists(game.GetSavePathForTesting(null)));

            game.ConfirmDeleteSaveForTesting(null);

            Assert.False(File.Exists(game.GetSavePathForTesting(null)));

            game.RenameSaveForTesting("save old to chapter one");

            Assert.False(File.Exists(game.GetSavePathForTesting("old")));
            Assert.True(File.Exists(game.GetSavePathForTesting("chapter one")));
            Assert.Contains("chapter-one", game.ListSaveSlotsForTesting());
        }
        finally
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public void FinalVictory_RequiresDefeatingFiveSquirrels()
    {
        var game = new Game(saveDirectory: null, random: new Random(0));
        game.SecureAllTreasuresForTesting();
        game.CurrentState.SquirrelsDefeated = 4;
        game.ForceSquirrelEncounterForTesting(5);

        game.AttackSquirrelForTesting();

        Assert.True(game.CurrentState.Won);
        Assert.Equal(5, game.CurrentState.SquirrelsDefeated);
        Assert.Null(game.CurrentState.ActiveSquirrelLevel);
    }

    private static string CaptureConsole(Action action)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();

        try
        {
            Console.SetOut(writer);
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}