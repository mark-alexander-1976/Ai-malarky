using TextAdventure.UI;

namespace TextAdventure.Engine;

public class Game
{
    private readonly GameState _state = new();
    private readonly GameRenderer _renderer = new();
    private readonly CommandHandler _commands;

    public Game()
    {
        WorldBuilder.Build(_state);
        _commands = new CommandHandler(_state, _renderer);
    }

    public void Run()
    {
        _renderer.PrintBanner();
        _commands.Look();

        while (!_state.GameOver)
        {
            var line = _renderer.ReadCommand();
            if (line is null) break;

            var command = CommandParser.Parse(line.Trim());
            if (command is not null)
                _commands.Execute(command);
        }
    }
}
