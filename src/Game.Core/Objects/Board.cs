public class Board{
    private readonly Dictionary<HexPosition, Unit> _positionsByUnits = new();
    private readonly Dictionary<Unit, HexPosition> _unitsByPosition = new();
    private readonly HexArea _area = new([]);
    public HexArea Area => _area;

    public bool IsOccupied(HexPosition position) => _positionsByUnits.ContainsKey(position);

    public bool HasUnit(Unit unit) => _unitsByPosition.ContainsKey(unit);

    public List<Unit> Units => _unitsByPosition.Keys.ToList();

    public Board(HexArea boardArea)
    {
        _area = boardArea;
    }

    public Unit? GetUnitAt(HexPosition position)
    {
        return _positionsByUnits.TryGetValue(position, out var unit) ? unit : null;
    }

    public List<Unit> GetUnitsAt(HexArea area)
    {
        var units = new List<Unit>();
        foreach(var position in area.Positions)
        {
            var unit = GetUnitAt(position);
            if(unit != null) units.Add(unit);
        }
        return units;
    }

    public HexPosition? GetPositionOf(Unit unit)
    {
        return _unitsByPosition.TryGetValue(unit, out var position) ? position : null;
    }

    public void Move(Unit unit, HexPosition from, HexPose to)
    {
        if(_positionsByUnits.ContainsKey(to.Position)) throw new Exception("trying to move unit onto occupied tile");
        _positionsByUnits.Remove(from);
        _positionsByUnits[to.Position] = unit;
        _unitsByPosition[unit] = to.Position;
        unit.Pose = to;
    }

    public bool IsInBounds(HexArea area)
    {
        return _area.Contains(area);
    }
    public void Move(Unit unit, HexPose from, HexPose to)
    {
        Move(unit, from.Position, to);
    }

    public void Place(Unit unit, HexPose at)
    {
        if(_positionsByUnits.ContainsKey(at.Position)) throw new Exception("trying to move unit onto occupied tile");
        _positionsByUnits[at.Position] = unit;
        _unitsByPosition[unit] = at.Position;
        unit.Pose = at;
    }

    public bool Remove(Unit unit)
    {
        if(!_unitsByPosition.TryGetValue(unit, out var position)) return false;
        _unitsByPosition.Remove(unit);
        _positionsByUnits.Remove(position);
        return true;
    }
}
