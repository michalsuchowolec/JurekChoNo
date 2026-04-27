using System.ComponentModel.DataAnnotations;
using Game.Core;

public abstract class Effect{
    public abstract void Enable(MatchEngine engine);
    public abstract void Disable(MatchEngine engine);
    public string Type = "";
}


public class DamageEffect : Effect
{
    public new string Type = "Damage";
    public Unit Unit;
    public int Damage;
    public TimeStamp TimeStamp;

    public DamageEffect(Unit unit, int damage, TimeStamp timeStamp){
        Unit = unit;
        Damage = damage;
        TimeStamp = timeStamp;
    }
    public override void Disable(MatchEngine engine)
    {
        engine.StatsEvaluator.CollectingHealthModifiers += ProvideModifier; 
    }

    public override void Enable(MatchEngine engine)
    {
        engine.StatsEvaluator.CollectingHealthModifiers -= ProvideModifier;
    }

    private void ProvideModifier(Unit unit, List<Modifier<int>> modifiers){
        if(Unit == unit){
            modifiers.Add(new AddModifier(-Damage, TimeStamp, false));
        }
    }
}

public class HealthAOEAura : Effect
{
    public new string Type = "HealthAOEAura";
    public Unit Source;
    public List<HexPos> Area;
    public int Health;

    public HealthAOEAura(Unit unit, int health, List<HexPos> area){
        Source = unit;
        Health = health;
        Area = area;
    }
    public override void Disable(MatchEngine engine)
    {
        engine.StatsEvaluator.CollectingHealthModifiers += ProvideModifier; 
    }

    public override void Enable(MatchEngine engine)
    {
        engine.StatsEvaluator.CollectingHealthModifiers -= ProvideModifier;
    }

    private void ProvideModifier(Unit unit, List<Modifier<int>> modifiers){
        if(DoesOverlap(unit)){
            modifiers.Add(new AddModifier(Health, new(), true));
        }
    }

    private bool DoesOverlap(Unit unit){
        foreach(var position in Area){
            var posTrans = new HexTransform(position, HexDirection.Up); 
            if(posTrans.Rebase(Source.Transform.BaseValue).Position == unit.Transform.BaseValue.Position) return true;
        }
        return false;
    }


}

public class DOTEffect : Effect{
    private string _type = "DOT"; 
    private Unit _unit;
    private int _damage;
    public DOTEffect(Unit unit, int damage, MatchEngine engine){
        _unit = unit;
        _damage = damage;
    }
    public override void Enable(MatchEngine engine)
    {
        engine.Events.TurnEnded += (turn) => DamageUnit(engine, turn);
    }

    public override void Disable(MatchEngine engine)
    {
        
    }

    private void DamageUnit(MatchEngine engine, int turn){

    }


}