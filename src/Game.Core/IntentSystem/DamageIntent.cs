namespace MatchEngine;

public record DamageDone(DamageIntent DamageIntent);

public class DamageIntent : Intent
{
    
    private Dictionary<Unit, int> _outcome;
    public IReadOnlyDictionary<Unit, int> Outcome => _outcome;

    private UnitSelector _targetSelector;
    public UnitSelector TargetSelector => _targetSelector;

    private DamageEvaluator _damageEvaluator;
    public DamageEvaluator DamageEvaluator => _damageEvaluator;

    public DamageIntent(EventBus events, UnitSelector targetSelector, DamageEvaluator damageEvaluator) : base(events)
    {
        _targetSelector = targetSelector;
        _damageEvaluator = damageEvaluator;
        targetSelector.Source = this;
        damageEvaluator.Source = this;
        _outcome = new();
    }

    public override void Perform()
    {
        var targets = _targetSelector.FindTargets();
        foreach (var target in targets)
        {
            var damage = _damageEvaluator.EvaluateDamage(target);
            target.TakeDamage(damage);
            _outcome[target] = damage;
        }
        _events.Publish<DamageDone>(new DamageDone(this));
    }
}
