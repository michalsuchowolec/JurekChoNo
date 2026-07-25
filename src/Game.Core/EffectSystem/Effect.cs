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
    private readonly EventBus _events;

    public DamageEffect(EntityId targetId, int damage, TimeStamp timeStamp, EventBus events){
        TargetId = targetId;
        Damage = damage;
        TimeStamp = timeStamp;
        _events = events;
    }
    public override void Enable()
    {
        _events.Subscribe<HealthModifiersRequested>(ProvideModifier);
    }
    public override void Disable()
    {
        _events.Unsubscribe<HealthModifiersRequested>(ProvideModifier);
    }
    private void ProvideModifier(HealthModifiersRequested request){
        if(request.Owner.Id == TargetId){
            request.Modifiers.Add(new DamageModifier(Damage, TimeStamp, false));
        }
    }
}

public class HealthAOEAura : Effect
{
    public new string Type = "HealthAOEAura";
    public Unit Source;
    public List<HexPosition> Area;
    public int Health;
    private readonly EventBus _events;

    public HealthAOEAura(Unit unit, int health, List<HexPosition> area, EventBus events){
        Source = unit;
        Health = health;
        Area = area;
        _events = events;
    }
    public override void Disable()
    {
        _events.Unsubscribe<HealthModifiersRequested>(ProvideModifier);
    }

    public override void Enable()
    {
        _events.Subscribe<HealthModifiersRequested>(ProvideModifier);
    }

    private void ProvideModifier(HealthModifiersRequested request){
        if(DoesOverlap(request.Owner)){
            request.Modifiers.Add(new BuffModifier(Health, new(), true));
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
    private readonly EventBus _events;

    public DOTEffect(Unit unit, int damage, EventBus events){
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
