using System.Data;
using Game.Core;
using MatchEngine;

public abstract class Intent{
    public string Type = "";
    public abstract void Perform(MomentResolver resolver);
}

public class DealDamageIntent : Intent{
    public new String Type = "DealDamage";
    public Unit Source;
    public Unit Target;
    public int BaseAmount;
    public int FinalDamage;
    private Stat<int> _amount;
    private List<Modifier<int>> _damageModifiers = new();

    public DealDamageIntent(Unit source, Unit target, int amount){
        Source = source;
        Target = target;
        BaseAmount = amount;
        _amount = new(BaseAmount);
    }
    public override void Perform(MomentResolver resolver)
    {
        //Check Validity

        //Enable Target Modification
        List<Modifier<Unit>> target = new(); //I don't know how to formulate this?
        
        //Enable Damage Modification
        resolver.Events.RaiseCollectIntentDamageModifiers(this);
        //Apply Damage
        if(target == null) return;
        var FinalDamage =  _amount.Evaluate(_damageModifiers);
        var effect = new DamageEffect(Target, FinalDamage, resolver.Now);
        Target.AddEffect(effect);
        resolver.Effects.QueueEffect(effect);
        //raise damage dealt event
        resolver.Events.RaiseDamageDone(this);
    }
}

public class MoveIntent : Intent
{
    public MoveIntent(Unit unit, HexTransform offset)
    {

    }
    public override void Perform(MomentResolver resolver)
    {
        Unit unit = new();
        throw new NotImplementedException();
    }
}


public class DamageEvaluator{
    
}