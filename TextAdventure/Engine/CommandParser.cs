namespace TextAdventure.Engine;

public record ParsedCommand(string Verb, string Noun);

public static class CommandParser
{
    public static ParsedCommand? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var verb = parts[0].ToUpperInvariant();
        var noun = parts.Length > 1 ? parts[1].ToUpperInvariant() : "";

        // Direction shortcuts → GO <direction>
        (verb, noun) = verb switch
        {
            "N" or "NORTH" => ("GO", "NORTH"),
            "S" or "SOUTH" => ("GO", "SOUTH"),
            "E" or "EAST"  => ("GO", "EAST"),
            "W" or "WEST"  => ("GO", "WEST"),
            "U" or "UP"    => ("GO", "UP"),
            "D" or "DOWN"  => ("GO", "DOWN"),
            _ => (verb, noun)
        };

        return new ParsedCommand(verb, noun);
    }
}
