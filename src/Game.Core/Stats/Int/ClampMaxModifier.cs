public class ClampMaxModifier : IntModifier
{
    public int Max;

    public ClampMaxModifier(int max, TimeStamp timeStamp, bool isAura)
        : base(IntModifierType.ClampMax, timeStamp, isAura, max)
    {
        Max = max;
    }

    public override int Transform(int initialValue) => Math.Min(initialValue, Max);
}
