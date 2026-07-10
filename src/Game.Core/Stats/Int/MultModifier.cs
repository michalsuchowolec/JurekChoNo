public class MultModifier : IntModifier
{
    public float Factor;

    public MultModifier(float factor, TimeStamp timeStamp, bool isAura)
        : base(IntModifierType.Mult, timeStamp, isAura, BitConverter.SingleToInt32Bits(factor))
    {
        Factor = factor;
    }

    public override int Transform(int initialValue) => (int)(initialValue * Factor);
}
