namespace MatchEngine;

public class CollectingDamageModifiers(DamageEvaluator evaluator, Unit target) : ModifierRequest<int>
{
    public DamageEvaluator Evaluator { get; } = evaluator;
    public Unit Target { get; } = target;
}

public abstract class DamageEvaluator
{
    private DamageIntent _source;
    public DamageIntent Source
    {
        get => _source;
        set => _source = value;
    }

    private EventBus _events;
    public EventBus Events => _events;

    public DamageEvaluator(EventBus events)
    {
        _events = events;
    }

    protected abstract int GetBaseDamage(Unit target);

    public int EvaluateDamage(Unit target)
    {
        var baseDamage = GetBaseDamage(target);
        var request = new CollectingDamageModifiers(this, target);
        _events.Publish(request);
        return Stat<int>.Evaluate(baseDamage, request.Modifiers);
    }
}

public class BasicDamageEvaluator : DamageEvaluator
{
    private int _baseDamage;
    public int BaseDamage => _baseDamage;

    public BasicDamageEvaluator(EventBus events, int baseDamage) : base(events)
    {
        _baseDamage = baseDamage;
    }

    protected override int GetBaseDamage(Unit target)
    {
        return _baseDamage;
    }
}

public class UserAttackDamageEvaluator : DamageEvaluator
{
    public UserAttackDamageEvaluator(EventBus events) : base(events)
    {
    }

    protected override int GetBaseDamage(Unit target)
    {
        return Source.UnitSource!.Attack;
    }
}

public class TargetHealthDamageEvaluator : DamageEvaluator
{
    private int _numerator;
    public int Numerator => _numerator;

    private int _denominator;
    public int Denominator => _denominator;

    public TargetHealthDamageEvaluator(EventBus events, int numerator, int denominator) : base(events)
    {
        _numerator = numerator;
        _denominator = denominator;
    }

    protected override int GetBaseDamage(Unit target)
    {
        return target.Health.Current * _numerator / _denominator;
    }
}
