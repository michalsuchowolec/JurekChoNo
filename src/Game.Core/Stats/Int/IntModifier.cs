public abstract class IntModifier : Modifier<int>
{
    public IntModifierType Type { get; }

    protected IntModifier(IntModifierType type, TimeStamp timeStamp, bool isAura, long valueSortKey = 0)
    {
        Type = type;
        Priority = new ModifierPriority(timeStamp, isAura, (int)type, valueSortKey);
    }
}

public enum IntModifierType
{
    Add,
    Mult,
    Div,
    ClampMin,
    ClampMax,
    Clamp
}
