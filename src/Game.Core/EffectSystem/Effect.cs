using System.ComponentModel.DataAnnotations;
using Game.Core;
using MatchEngine;

public abstract class Effect{
    public abstract void Enable(GameEventSystem events);
    public abstract void Disable(GameEventSystem events);
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
    public override void Disable(GameEventSystem events)
    {
        events.CollectingHealthModifiers -= ProvideModifier;
    }

    public override void Enable(GameEventSystem events)
    {
        events.CollectingHealthModifiers += ProvideModifier;
    }
    private void ProvideModifier(Unit unit, IList<Modifier<Health>> modifiers){
        if(Unit == unit){
            modifiers.Add(new DamageModifier(-Damage, TimeStamp, false));
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
    public override void Disable(GameEventSystem events)
    {
        events.CollectingHealthModifiers -= ProvideModifier;
    }

    public override void Enable(GameEventSystem events)
    {
        events.CollectingHealthModifiers += ProvideModifier;
    }

    private void ProvideModifier(Unit unit, IList<Modifier<Health>> modifiers){
        if(DoesOverlap(unit)){
            modifiers.Add(new BuffModifier(Health, new(), true));
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
    public DOTEffect(Unit unit, int damage){
        _unit = unit;
        _damage = damage;
    }
    public override void Enable(GameEventSystem events)
    {
        //events.TurnEnded += (turn) => DamageUnit(events, turn);
    }

    public override void Disable(GameEventSystem events)
    {

    }

    private void DamageUnit(GameEventSystem events, int turn){

    }


}
