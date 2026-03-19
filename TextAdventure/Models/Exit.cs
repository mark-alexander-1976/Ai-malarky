namespace TextAdventure.Models;

public record Exit(string Direction, int RoomId, string? RequiredItem = null, string? BlockedMessage = null);
