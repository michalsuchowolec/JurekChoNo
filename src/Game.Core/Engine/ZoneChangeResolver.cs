namespace MatchEngine;

public class ZoneChangeRequested
{
    public ZoneChangeRequested(Unit unit, CardsContainer destination, Intent? intent)
    {
        Unit = unit;
        OriginalDestination = destination;
        Destination = destination;
        Intent = intent;
    }
    public Unit Unit;
    public Intent? Intent;
    public CardsContainer OriginalDestination { get; }
    private CardsContainer _destination = null!;
    public CardsContainer Destination{get{return _destination;} set
        {
            if(_destination is null || CardsContainer.HasPriority(value, _destination, Unit.Owner)) _destination = value;
        }
    }
}

public class ZoneChangeResolver{
    private readonly MatchRoot _matchRoot;
    private Board Board => _matchRoot.MatchState.Board;
    private Dictionary<Unit, CardsContainer> _queuedZoneChanges = new();

    public ZoneChangeResolver(MatchRoot matchRoot)
    {
        _matchRoot = matchRoot;
    }

    public bool ProcessZoneChanges(){
        CheckDeathConditions();
        if(_queuedZoneChanges.Count == 0) return false;
        foreach(var unit in _queuedZoneChanges.Keys)
        {
            if(!Board.Units.Contains(unit)) continue;
            Board.Remove(unit);
            _queuedZoneChanges[unit].Add(UnitCard.FromUnit(unit, _matchRoot));
        }
        _queuedZoneChanges.Clear();
        return true;
    }

    public void CheckDeathConditions()
    {
        foreach(var unit in Board.Units)
        {
            if(unit.Health.Current < 0) QueueZoneChange(unit, unit.Owner.DiscardPile);
        }
    }

    public void QueueZoneChange(Unit unit, CardsContainer destination, Intent? intent = null)
    {
        var request = new ZoneChangeRequested(unit, destination, intent);
        _matchRoot.Events.Publish(request);
        if(!_queuedZoneChanges.TryGetValue(unit, out var current) || CardsContainer.HasPriority(request.Destination, current, unit.Owner))
        {
            _queuedZoneChanges[unit] = request.Destination;
        }
    }
}
