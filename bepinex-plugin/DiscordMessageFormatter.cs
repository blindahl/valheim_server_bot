using System.Collections.Generic;
using System.Linq;

namespace BepInExDiscordConnect;

public static class DiscordMessageFormatter
{
    public static string FormatPlayerJoined(string player, IReadOnlyCollection<string> activePlayers)
    {
        return FormatPlayerChange("Player joined", player, activePlayers);
    }

    public static string FormatPlayerLeft(string player, IReadOnlyCollection<string> activePlayers)
    {
        return FormatPlayerChange("Player left", player, activePlayers);
    }

    private static string FormatPlayerChange(string eventText, string player, IReadOnlyCollection<string> activePlayers)
    {
        var playerList = activePlayers.Count == 0 ? "none" : string.Join("\n", activePlayers);
        return $"{eventText}: {player}\nActive players ({activePlayers.Count}):\n{playerList}";
    }
}
