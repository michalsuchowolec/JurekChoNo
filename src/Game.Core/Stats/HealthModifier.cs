public struct Health{
    public int Max;
    public int Current;

    public Health(int max){
        Max = max;
        Current = max;
    }

    public Health(int max, int current){
        Max = max;
        Current = current;
    }
}

public enum HealthModifierType{
    Damage,
    Heal,
    Buff,
    Set
}

public abstract class HealthModifier : Modifier<Health>{
    public HealthModifierType Type;
}

public class DamageModifier : HealthModifier{
    public new HealthModifierType Type = HealthModifierType.Damage;

    public int Amount;

    public DamageModifier(int amount, TimeStamp timeStamp, bool isAura){
        Amount = amount;
        Priority = new ModifierPriority(timeStamp, isAura, (int)Type, amount);
    }

    public override Health Transform(Health initialState)
    {
        return new Health{
            Max = initialState.Max,
            Current = initialState.Current - Amount
        };
    }
}

public class HealModifier : HealthModifier{
    public new HealthModifierType Type = HealthModifierType.Heal;

    public int Amount;

    public HealModifier(int amount, TimeStamp timeStamp, bool isAura){
        Amount = amount;
        Priority = new ModifierPriority(timeStamp, isAura, (int)Type, amount);
    }

    public override Health Transform(Health initialState)
    {
        return new Health{
            Max = initialState.Max,
            Current = Math.Min(initialState.Current + Amount, initialState.Max)
        };
    }
}

public class BuffModifier : HealthModifier {
    public new HealthModifierType Type = HealthModifierType.Buff;
    public int Amount;

    public BuffModifier(int amount, TimeStamp timeStamp, bool isAura){
        Amount = amount;
        Priority = new ModifierPriority(timeStamp, isAura, (int)Type, amount);
    }

    public override Health Transform(Health initialState)
    {
        return new Health{
            Max = initialState.Max + Amount,
            Current = initialState.Current + Amount
        };
    }
}

public class SetModifier : HealthModifier{
    public new HealthModifierType Type = HealthModifierType.Set;
    public int Value;

    public SetModifier(int value, TimeStamp timeStamp, bool isAura){
        Value = value;
        Priority = new ModifierPriority(timeStamp, isAura, (int)Type, value);
    }

    public override Health Transform(Health initialState)
    {
        return new Health{
            Max = Value,
            Current = Value
        };
    }
}
