namespace MatchEngine;

public class CollectingHexAreaModifiers(AreaSelector selector) : ModifierRequest<HexArea>
{
    public AreaSelector Selector { get; } = selector;
}

public class AreaSelector : UnitSelector
{
    private HexArea _area;
    public HexArea Area => _area;

    // Needs both the board and events - rather than collecting the two separately, it just asks
    // the moment for both, same as CollisionBehaviour does.
    private MomentResolver _momentResolver;
    public Board Board => _momentResolver.MatchRoot.MatchState.Board;
    public EventBus Events => _momentResolver.Events;

    public AreaSelector(HexArea area, MomentResolver momentResolver)
    {
        _area = area;
        _momentResolver = momentResolver;
    }

    public override List<Unit> FindTargets()
    {
        var request = new CollectingHexAreaModifiers(this);
        Events.Publish(request);
        return Board.GetUnitsAt(Stat<HexArea>.Evaluate(_area, request.Modifiers));
    }
}
