using System.Text;

public class HexArea : IEquatable<HexArea>
{
    private HashSet<HexPosition> _tiles = new();
    public void Add(HexArea other)
    {
        foreach(var tile in other._tiles)
        {
            _tiles.Add(tile);
        }    
    }

    public void Add(HexPosition tile)
    {
        _tiles.Add(tile);
    }

    public void Transform(HexPose origin)
    {
        var newTiles = new HashSet<HexPosition>();
        foreach(var tile in _tiles)
        {
            newTiles.Add(origin * tile);
        }
        _tiles = newTiles;
    }

    public void Offset(HexPosition offset)
    {
        var newTiles = new HashSet<HexPosition>();
        foreach(var tile in _tiles)
        {
            newTiles.Add(tile + offset);
        }
        _tiles = newTiles;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        var tiles = _tiles.ToArray();
        Array.Sort(tiles);
        return $"({string.Join(", ", tiles)})";
    }

    public bool Equals(HexArea? other)
    {
        if(other == null) return false;
        if(_tiles.Count != other._tiles.Count) return false;
        foreach(var tile in _tiles)
        {
            if(!other._tiles.Contains(tile)) return false;
        }
        return true;
    }
}
