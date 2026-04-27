namespace Game.Core;

/// <summary>
/// Unique identifier for a player in the game.
/// </summary>
public readonly record struct PlayerId(int Value)
{
    public override string ToString() => $"Player{Value}";
}

/// <summary>
/// Unique identifier for any game entity (unit, building, token, etc.).
/// </summary>
public readonly record struct EntityId(int Value)
{
    public override string ToString() => $"Entity{Value}";
}
