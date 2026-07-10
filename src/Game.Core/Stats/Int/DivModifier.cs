public class DivModifier : IntModifier
{
    public float Divisor;

    public DivModifier(float divisor, TimeStamp timeStamp, bool isAura)
        : base(IntModifierType.Div, timeStamp, isAura, BitConverter.SingleToInt32Bits(divisor))
    {
        Divisor = divisor;
    }

    public override int Transform(int initialValue) => (int)(initialValue / Divisor);
}
