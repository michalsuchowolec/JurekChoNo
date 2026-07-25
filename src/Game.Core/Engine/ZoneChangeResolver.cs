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
    private Board _board;
    private EventBus _events;
    private Dictionary<Unit, CardsContainer> _queuedZoneChanges = new();

    public ZoneChangeResolver(Board board, EventBus events)
    {
        _board = board;
        _events = events;
    }

    public bool ProcessZoneChanges(){
        CheckDeathConditions();
        if(_queuedZoneChanges.Count == 0) return false;
        foreach(var unit in _queuedZoneChanges.Keys)
        {
            if(!_board.Units.Contains(unit)) continue;
            _board.Remove(unit);
            _queuedZoneChanges[unit].Add(UnitCard.FromUnit(unit));
        }
        _queuedZoneChanges.Clear();
        return true;
    }

    public void CheckDeathConditions()
    {
        foreach(var unit in _board.Units)
        {
            if(unit.Health.Current < 0) QueueZoneChange(unit, unit.Owner.DiscardPile);
        }
    }

    public void QueueZoneChange(Unit unit, CardsContainer destination, Intent? intent = null)
    {
        var request = new ZoneChangeRequested(unit, destination, intent);
        _events.Publish(request);
        if(!_queuedZoneChanges.TryGetValue(unit, out var current) || CardsContainer.HasPriority(request.Destination, current, unit.Owner))
        {
            _queuedZoneChanges[unit] = request.Destination;
        }
    }
}
