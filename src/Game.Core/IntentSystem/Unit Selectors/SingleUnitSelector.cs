namespace MatchEngine;

public class SingleUnitSelector : UnitSelector
{
    private Unit _unit;
    public Unit Unit => _unit;

    public SingleUnitSelector(Unit unit)
    {
        _unit = unit;
    }

    public override List<Unit> FindTargets()
    {
        return new List<Unit> { _unit };
    }
}
