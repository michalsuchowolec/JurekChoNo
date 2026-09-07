public enum BoolModifierType
{
    SetFalse,
    SetTrue
}

public abstract class BoolModifier : Modifier<bool>
{
    public BoolModifierType Type { get; }

    protected BoolModifier(BoolModifierType type, TimeStamp timeStamp, bool isAura)
    {
        Type = type;
        // No special override tier - this follows the same rules as every other modifier type.
        // An aura always beats a non-aura (IsAura), and between modifiers that share aura-ness
        // the more recent one wins (TimeStamp), regardless of direction. Only a genuine tie -
        // same IsAura, same TimeStamp - falls through to OperationPriority, where SetTrue (1)
        // sorts after SetFalse (0) and wins. "Can always move, regardless of other effects" isn't
        // a special escape hatch; it's just describing the effect as an aura, so it wins on
        // recency like anything else meant to persist.
        Priority = new ModifierPriority(timeStamp, isAura, (int)type);
    }
}

public class SetFalseModifier : BoolModifier
{
    public SetFalseModifier(TimeStamp timeStamp, bool isAura) : base(BoolModifierType.SetFalse, timeStamp, isAura) { }

    public override bool Transform(bool initialState) => false;
}

public class SetTrueModifier : BoolModifier
{
    public SetTrueModifier(TimeStamp timeStamp, bool isAura) : base(BoolModifierType.SetTrue, timeStamp, isAura) { }

    public override bool Transform(bool initialState) => true;
}
