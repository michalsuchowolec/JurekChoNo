using System.Runtime.CompilerServices;

public abstract class IntModifier : Modifier<int>{
    public IntModifierType Type; 
}

public class AddModifier : IntModifier{
    public new IntModifierType Type = IntModifierType.Add;
    public int ValueToAdd;

    public AddModifier(int valueToAdd, TimeStamp timeStamp, bool isAura){
        ValueToAdd = valueToAdd;
        Priority = new ModifierPriority(timeStamp, isAura, (int) Type, valueToAdd);
    }
    public override int Transform(int initialValue)
    {
        return initialValue + ValueToAdd;
    }
}

public class MultModifier : IntModifier{
    public new IntModifierType Type = IntModifierType.Mult;
    public float Factor;

    public MultModifier(float factor, TimeStamp timeStamp, bool isAura){
        Factor = factor;
        Priority = new ModifierPriority(timeStamp, isAura, (int) Type, BitConverter.SingleToInt32Bits(factor));
    }
    public override int Transform(int initialValue)
    {
        return (int)(initialValue * Factor);
    }
}

public class DivModifier : IntModifier{
    public new IntModifierType Type = IntModifierType.Div;
    public float Divisor;

    public DivModifier(float divisor, TimeStamp timeStamp, bool isAura){
        Divisor = divisor;
        Priority = new ModifierPriority(timeStamp, isAura, (int) Type, BitConverter.SingleToInt32Bits(divisor));
    }
    public override int Transform(int initialValue)
    {
        return (int)(initialValue / Divisor);
    }
}

public class ClampMinModifier : IntModifier{
    public new IntModifierType Type = IntModifierType.ClampMin;
    public int Min;

    public ClampMinModifier(int min, TimeStamp timeStamp, bool isAura){
        Min = min;
        Priority = new ModifierPriority(timeStamp, isAura, (int) Type, min);
    }
    public override int Transform(int initialValue)
    {
        return Math.Max(initialValue, Min);
    }
}

public class ClampMaxModifier : IntModifier{
    public new IntModifierType Type = IntModifierType.ClampMax;
    public int Max;

    public ClampMaxModifier(int max, TimeStamp timeStamp, bool isAura){
        Max = max;
        Priority = new ModifierPriority(timeStamp, isAura, (int) Type, max);
    }
    public override int Transform(int initialValue)
    {
        return Math.Min(initialValue, Max);
    }
}

public class ClampModifier : IntModifier{
    public new IntModifierType Type = IntModifierType.Clamp;
    public int Min;
    public int Max;

    public ClampModifier(int min, int max, TimeStamp timeStamp, bool isAura){
        Min = min;
        Max = max;
        Priority = new ModifierPriority(timeStamp, isAura, (int) Type, ModifierPriority.PackIntPair(min, max));
    }
    public override int Transform(int initialValue)
    {
        return Math.Clamp(initialValue, Min, Max);
    }
}


public enum IntModifierType{
    Add,
    Mult,
    Div,
    ClampMin,
    ClampMax,
    Clamp
}
