namespace Game.Core;

public readonly record struct PlayerId(int Value)
{
    public override string ToString() => $"Player{Value}";
}

public readonly record struct EntityId(int Value)
{
    public override string ToString() => $"Entity{Value}";
}

public readonly record struct EffectId(int Value)
{
    public override string ToString() => $"Effect{Value}";
}

public class IdManager
{
    private int _nextEntityId = 1;
    private int _nextEffectId = 1;
    private int _nextPlayerId = 1;

    public EntityId NextEntityId() => new(_nextEntityId++);
    public EffectId NextEffectId() => new(_nextEffectId++);
    public PlayerId NextPlayerId() => new(_nextPlayerId++);
}
