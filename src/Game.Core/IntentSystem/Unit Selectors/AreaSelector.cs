namespace MatchEngine;

public class CollectingHexAreaModifiers(AreaSelector selector) : ModifierRequest<HexArea>
{
    public AreaSelector Selector { get; } = selector;
}

public class AreaSelector : UnitSelector
{
    private HexArea _area;
    public HexArea Area => _area;

    private Board _board;
    public Board Board => _board;

    private EventBus _events;
    public EventBus Events => _events;

    public AreaSelector(HexArea area, Board board, EventBus events)
    {
        _area = area;
        _board = board;
        _events = events;
    }

    public override List<Unit> FindTargets()
    {
        var request = new CollectingHexAreaModifiers(this);
        _events.Publish(request);
        return _board.GetUnitsAt(Stat<HexArea>.Evaluate(_area, request.Modifiers));
    }
}
