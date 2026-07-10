using Game.Core;
using MatchEngine;


public class Unit{
    public readonly EntityId Id;
    private readonly MomentResolver _resolver;
    private readonly HealthStat _health = new(100);
    private readonly IntStat _speed = new(3);
    private readonly IntStat _attack = new(5);
    public readonly HexPoseStat CurrentPose = new(new HexPose());
    private List<Effect> _effects = new();
    public Player Owner;
    private List<CollisionBehaviour> _collisionBehaviours;


    public Unit(EntityId id, MomentResolver resolver){
        Id = id;
        _resolver = resolver;
    }

    public Health Health => _health.Evaluate(_resolver.StatEvaluator.CollectHealthModifiers(this));
    public int Attack => _attack.Evaluate(_resolver.StatEvaluator.CollectAttackModifiers(this));
    public int Speed => _speed.Evaluate(_resolver.StatEvaluator.CollectSpeedModifiers(this));
    public HexPose Pose => CurrentPose.Evaluate([]);

    public void AddEffect(Effect effect){
        _effects.Add(effect);
    }

    public void Collide(Unit other)
    {
        foreach(var collisionBehaviour in _collisionBehaviours)
        {
            if (collisionBehaviour.Validate(this, other))
            {
                collisionBehaviour.Perform(this, other);
            }
        }
    }
}


public class Player
{

}


public class CollisionResolver
{

}

public abstract class CollisionBehaviour
{
    protected MomentResolver _momentResolver;
    public CollisionBehaviourType Type = CollisionBehaviourType.Undefined;
    public CollisionBehaviour(MomentResolver momentResolver)
    {
        _momentResolver = momentResolver;
    }
    public abstract bool Validate(Unit user, Unit target);
    public abstract void Perform(Unit user, Unit target);

}

public enum CollisionBehaviourType
{
    Undefined,
    BasicAttack,

}

public class BasicAttackBehaviour : CollisionBehaviour
{

    public BasicAttackBehaviour(MomentResolver momentResolver) : base(momentResolver)
    {
        Type = CollisionBehaviourType.BasicAttack;
    }
    public override bool Validate(Unit user, Unit target)
    {
        return user != target;
    }
    public override void Perform(Unit user, Unit target)
    {
        _momentResolver.Intents.QueueIntent(new DealDamageIntent(user, target, user.Attack));
    }
}
