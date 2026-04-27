namespace Game.Core;

/// <summary>
/// Authoritative game state containing all information, including secrets.
/// This is the "god view" - never expose directly to players.
/// </summary>
public sealed class GameState
{
    public GameSeed Seed { get; init; }
    public int TurnNumber { get; set; } = 1;
    public PlayerId CurrentPlayer { get; set; } = new(0);
    public IReadOnlyList<PlayerId> Players { get; init; } = [];

    // TODO: Add your game-specific state here:
    // - Board/map data
    // - Entity collections
    // - Hidden information (hands, decks, fog of war)
    // - Resource pools
    // - Victory conditions

    public static GameState CreateNew(GameSeed seed, int playerCount)
    {
        var players = Enumerable.Range(0, playerCount)
            .Select(i => new PlayerId(i))
            .ToList();

        return new GameState
        {
            Seed = seed,
            Players = players,
            CurrentPlayer = players[0]
        };
    }
}

/// <summary>
/// Redacted view of the game state for a specific player.
/// Contains only information that player is allowed to see.
/// </summary>
public sealed class PlayerView
{
    public required PlayerId ViewingPlayer { get; init; }
    public int TurnNumber { get; init; }
    public PlayerId CurrentPlayer { get; init; }
    public IReadOnlyList<PlayerId> Players { get; init; } = [];

    // TODO: Add visible state for the player:
    // - Visible map cells
    // - Known enemy positions
    // - Own resources/hand
    // - Public information
}
