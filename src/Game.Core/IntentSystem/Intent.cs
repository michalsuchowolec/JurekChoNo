using System.Data;
using Game.Core;
using MatchEngine;

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

    private GameEventSystem _eventSystem;
    public DealDamageIntent(Unit source, Unit target, int amount, GameEventSystem eventSystem){
        Source = source;
        Target = target;
        BaseAmount = amount;
        _eventSystem = eventSystem;
    }
    public override void Perform()
    {
        throw new NotImplementedException();
        //Check Validity
        //Enable Area Modification
        //Enable Damage Modification
        //Apply Damage
        //raise damage dealt event
    }
}

public class MoveIntent : Intent
{
    public MoveIntent(Unit unit, HexTransform offset, GameEventSystem eventSystem)
    {
        
    }
    public override void Perform()
    {
        Unit unit = new();
        throw new NotImplementedException();

    }
}


public class DamageEvaluator{
    
}