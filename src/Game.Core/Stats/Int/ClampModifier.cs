public class ClampModifier : IntModifier
{
    public int Min;
    public int Max;

    public ClampModifier(int min, int max, TimeStamp timeStamp, bool isAura)
        : base(IntModifierType.Clamp, timeStamp, isAura, ModifierPriority.PackIntPair(min, max))
    {
        Min = min;
        Max = max;
    }

    public override int Transform(int initialValue) => Math.Clamp(initialValue, Min, Max);
}
