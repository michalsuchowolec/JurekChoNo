using Game.Core;
using MatchEngine;


public class Unit : IDamagable, IEffectsOwner{
    public readonly EntityId Id;
    private readonly EventBus _events;
    private readonly Stat<Health> _health = new(new Health(100));
    private readonly Stat<int> _speed = new(3);
    private readonly Stat<int> _attack = new(5);
    public  HexPose Pose = HexPose.Identity;
    private List<Effect> _effects = new();
    public IEnumerable<Effect> Effects => _effects;
    public Player Owner;
    private List<CollisionBehaviour> _collisionBehaviours;
    public UnitZone Zone;
    public string Template;
    private Stat<HexArea> _collisionArea = new(HexArea.Circle(0)); //colossal
    private Stat<HexArea> _enemyCollisionArea = new(HexArea.Circle(0)); //taunt

    private Stat<bool> _canExhaust = new(false);
    public bool IsExhausted = false;
    public bool IsAlive => Zone == UnitZone.Board;
    public Unit(EntityId id, EventBus events){
        Id = id;
        _events = events;
        _collisionBehaviours = new();
    }

    public Health Health {
        get {
            var request = new HealthModifiersRequested(this);
            _events.Publish(request);
            return _health.Evaluate(request.Modifiers);
        }
    }
    public int Attack {
        get {
            var request = new AttackModifiersRequested(this);
            _events.Publish(request);
            return _attack.Evaluate(request.Modifiers);
        }
    }
    public int Speed {
        get {
            var request = new SpeedModifiersRequested(this);
            _events.Publish(request);
            return _speed.Evaluate(request.Modifiers);
        }
    }
    public HexArea CollisionArea => _collisionArea.Evaluate(new()); //TODO
    public HexArea EnemyCollisionArea => _enemyCollisionArea.Evaluate(new()); //TODO
    public bool CanExhaust => _canExhaust.Evaluate(new()); //TODO

    EntityId IDamagable.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


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

    public bool CanMove(int Initiative)
    {
        return !IsExhausted && Speed >= Initiative;
    }

    public void TakeDamage(int amount)
    {
        throw new NotImplementedException();
    }

}

public record PlansToMoveRequested();

public enum UnitZone
{
    Board, //behaves as unit on board.
    Card //when moved elsewhere turns back into card.
    
    //Maybe some other possible states. 
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
        var selector = new SingleUnitSelector(target);
        var evaluator = new BasicDamageEvaluator(_momentResolver.Events, user.Attack);
        _momentResolver.Intents.QueueIntent(new DamageIntent(_momentResolver.Events, selector, evaluator));
    }
}
