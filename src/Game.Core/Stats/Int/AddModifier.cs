public class AddModifier : IntModifier
{
    public int ValueToAdd;

    public AddModifier(int valueToAdd, TimeStamp timeStamp, bool isAura)
        : base(IntModifierType.Add, timeStamp, isAura, valueToAdd)
    {
        ValueToAdd = valueToAdd;
    }

    public override int Transform(int initialValue) => initialValue + ValueToAdd;
}
