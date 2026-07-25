namespace MatchEngine;
public abstract class Intent
{
    protected List<string> _tags = new();
    public List<string> Tags => _tags;

    protected Unit? _unitSource;
    public Unit? UnitSource => _unitSource;

    protected Card? _cardSource;
    public Card? CardSource => _cardSource;

    protected Effect? _effectSource;
    public Effect? EffectSource => _effectSource;

    protected EventBus _events;
    public EventBus Events => _events;

    public Intent(EventBus events)
    {
        _events = events;
    }
    public abstract void Perform();
}
