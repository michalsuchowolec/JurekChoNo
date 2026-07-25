public struct ResourceSymbol : IEquatable<ResourceSymbol>
{
    private int _bitMask;

    public static readonly ResourceSymbol Any = new((ResourceType[])Enum.GetValues(typeof(ResourceType)));

    public ResourceSymbol(IEnumerable<ResourceType> types)
    {
        _bitMask = 0;
        foreach(var type in types)
        {
            var curMask = 1 << Convert.ToInt32(type);
            if((_bitMask & curMask) > 0) throw new Exception("repeating types");
            _bitMask += curMask;
        }
    }

    public readonly bool HasType(ResourceType type) => (_bitMask & (1 << Convert.ToInt32(type))) != 0;

    public readonly List<ResourceType> GetTypes()
    {
        var result = new List<ResourceType>();
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (HasType(type)) result.Add(type);
        }
        return result;
    }

    public bool Overlaps(ResourceSymbol other)
    {
        return (_bitMask & other._bitMask) > 0;
    }

    public readonly bool Equals(ResourceSymbol other) => _bitMask == other._bitMask;

    public override readonly bool Equals(object? obj) => obj is ResourceSymbol other && Equals(other);

    public override readonly int GetHashCode() => _bitMask;

    public static bool operator ==(ResourceSymbol left, ResourceSymbol right) => left.Equals(right);

    public static bool operator !=(ResourceSymbol left, ResourceSymbol right) => !left.Equals(right);
}
