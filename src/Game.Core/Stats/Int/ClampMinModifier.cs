public class ClampMinModifier : IntModifier
{
    public int Min;

    public ClampMinModifier(int min, TimeStamp timeStamp, bool isAura)
        : base(IntModifierType.ClampMin, timeStamp, isAura, min)
    {
        Min = min;
    }

    public override int Transform(int initialValue) => Math.Max(initialValue, Min);
}
