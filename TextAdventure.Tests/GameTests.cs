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
}