using Game.Core;
using MatchEngine;

public abstract class Effect{
    public abstract void Enable();
    public abstract void Disable();
    public string Type = "";
}


public class DamageEffect : Effect
{
    public new string Type = "Damage";
    public EntityId TargetId;
    public int Damage;
    public TimeStamp TimeStamp;
    private readonly StatEvaluator _statEvaluator;

    public DamageEffect(EntityId targetId, int damage, TimeStamp timeStamp, StatEvaluator statEvaluator){
        TargetId = targetId;
        Damage = damage;
        TimeStamp = timeStamp;
        _statEvaluator = statEvaluator;
    }
    public override void Enable()
    {
        _statEvaluator.HealthModifiersRequested += ProvideModifier;
    }
    public override void Disable()
    {
        _statEvaluator.HealthModifiersRequested -= ProvideModifier;
    }
    private void ProvideModifier(Unit unit, List<Modifier<Health>> modifiers){
        if(unit.Id == TargetId){
            modifiers.Add(new DamageModifier(Damage, TimeStamp, false));
        }
    }
}

public class HealthAOEAura : Effect
{
    public new string Type = "HealthAOEAura";
    public Unit Source;
    public List<HexPosition> Area;
    public int Health;
    private readonly StatEvaluator _statEvaluator;

    public HealthAOEAura(Unit unit, int health, List<HexPosition> area, StatEvaluator statEvaluator){
        Source = unit;
        Health = health;
        Area = area;
        _statEvaluator = statEvaluator;
    }
    public override void Disable()
    {
        _statEvaluator.HealthModifiersRequested -= ProvideModifier;
    }

    public override void Enable()
    {
        _statEvaluator.HealthModifiersRequested += ProvideModifier;
    }

    private void ProvideModifier(Unit unit, List<Modifier<Health>> modifiers){
        if(DoesOverlap(unit)){
            modifiers.Add(new BuffModifier(Health, new(), true));
        }
    }

    private bool DoesOverlap(Unit unit){
        foreach(var position in Area){
            if(HexPosition.Transform(Source.Pose, position).Equals(unit.Pose.Position)) return true;
        }
        return false;
    }
}

public class DOTEffect : Effect{
    private Unit _unit;
    private int _damage;
    private readonly GameEventSystem _events;

    public DOTEffect(Unit unit, int damage, GameEventSystem events){
        _unit = unit;
        _damage = damage;
        _events = events;
    }
    public override void Enable()
    {
        //events.TurnEnded += DamageUnit;
    }

    public override void Disable()
    {

    }

    private void DamageUnit(){

    }
}
