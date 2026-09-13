using BepInExDiscordConnect;
using Xunit;

namespace BepInExDiscordConnect.Tests;

public sealed class DiscordMessageFormatterTests
{
    [Fact]
    public void FormatPlayerJoined_ListsJoiningPlayerAndActivePlayersOnSeparateLines()
    {
        var message = DiscordMessageFormatter.FormatPlayerJoined(
            "Alice",
            new[] { "Alice", "Bob" });

        Assert.Equal(
            "Player joined: Alice\nActive players (2):\nAlice\nBob",
            message);
    }

    [Fact]
    public void FormatPlayerJoined_ReportsNoneWhenThereAreNoActivePlayers()
    {
        var message = DiscordMessageFormatter.FormatPlayerJoined(
            "Alice",
            Array.Empty<string>());

        Assert.Equal(
            "Player joined: Alice\nActive players (0):\nnone",
            message);
    }

    [Fact]
    public void FormatPlayerLeft_ListsRemainingPlayersOnSeparateLines()
    {
        var message = DiscordMessageFormatter.FormatPlayerLeft(
            "Alice",
            new[] { "Bob", "Charlie" });

        Assert.Equal(
            "Player left: Alice\nActive players (2):\nBob\nCharlie",
            message);
    }
}
