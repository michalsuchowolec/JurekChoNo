using System.Security.Cryptography;
using System.Text;

public class HexArea : IEquatable<HexArea>
{
    

    private HashSet<HexPosition> _positions = new();
    public List<HexPosition> Positions => _positions.ToList();

    public static HexArea Circle(int radius)
    {
        var oldArea = new HashSet<HexPosition>{HexPosition.Identity};
        var directions = new HexPosition[]
        {
            HexPosition.Up,
            HexPosition.UpRight,
            HexPosition.DownRight,
            HexPosition.Down,
            HexPosition.DownLeft,
            HexPosition.UpLeft
        };
        for(int i = 0; i < radius; i++)
        {
            var newArea = new HashSet<HexPosition>();
            foreach(var point in oldArea)
            {
                foreach(var direction in directions)
                {
                    newArea.Add(point + direction);
                }
            }
            oldArea = newArea;
        }
        return new HexArea(oldArea);
    }

    public HexArea(IEnumerable<HexPosition> coordinates)
    {
        foreach(var coordinate in coordinates)
        {
            _positions.Add(coordinate);       
        }
    }
    public void Add(HexArea other)
    {
        foreach(var tile in other._positions)
        {
            _positions.Add(tile);
        }    
    }

    public void Add(HexPosition tile)
    {
        _positions.Add(tile);
    }

    public void Transform(HexPose origin)
    {
        var newTiles = new HashSet<HexPosition>();
        foreach(var tile in _positions)
        {
            newTiles.Add(origin * tile);
        }
        _positions = newTiles;
    }

    public void Offset(HexPosition offset)
    {
        var newTiles = new HashSet<HexPosition>();
        foreach(var tile in _positions)
        {
            newTiles.Add(tile + offset);
        }
        _positions = newTiles;
    }

    public bool Overlaps(HexArea other)
    {
        return _positions.Overlaps(other._positions);
    }

    public bool Contains(HexArea other)
    {
        return _positions.IsSupersetOf(other._positions);
    }

    public static HexArea operator *(HexPose origin, HexArea area)
    {
        var result = new HexArea(area._positions);
        result.Transform(origin);
        return result;
    }

    public int Distance(HexArea other)
    {
        var minDistance = int.MaxValue;
        foreach(var a in _positions)
        {
            foreach(var b in other._positions)
            {
                minDistance = Math.Min(minDistance, (a - b).Magnitude);
            }
        }
        return minDistance;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        var tiles = _positions.ToArray();
        Array.Sort(tiles);
        return $"({string.Join(", ", tiles)})";
    }

    public bool Equals(HexArea? other)
    {
        if(other == null) return false;
        if(_positions.Count != other._positions.Count) return false;
        foreach(var tile in _positions)
        {
            if(!other._positions.Contains(tile)) return false;
        }
        return true;
    }
}
