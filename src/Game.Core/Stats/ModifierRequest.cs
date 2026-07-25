namespace MatchEngine;

public abstract class ModifierRequest<TValue>
{
    public List<Modifier<TValue>> Modifiers { get; } = new();
}

public class HealthModifiersRequested(Unit owner) : ModifierRequest<Health>
{
    public Unit Owner { get; } = owner;
}
public class AttackModifiersRequested(Unit owner) : ModifierRequest<int>
{
    public Unit Owner { get; } = owner;
}
public class SpeedModifiersRequested(Unit owner) : ModifierRequest<int>
{
    public Unit Owner { get; } = owner;
}
