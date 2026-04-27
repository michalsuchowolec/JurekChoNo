using System.Data;

public abstract class Intent{
    public string Type = "";
    public abstract void Perform();
}

public class DealDamageIntent : Intent{
    public new String Type = "DealDamage";
    public Unit Source;
    public Unit Target;
    public int BaseAmount;
    private Stat<int> _amount; 
    public DealDamageIntent(Unit source, Unit target, int amount){
        Source = source;
        Target = target;
        BaseAmount = amount;
    }
    public override void Perform()
    {
        throw new NotImplementedException();
    }
}


public class DamageEvaluator{
    
}