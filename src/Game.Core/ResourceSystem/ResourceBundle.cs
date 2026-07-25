using System.Collections.Immutable;

public sealed class ResourceBundle : IEquatable<ResourceBundle>
{
    public static readonly ResourceBundle Empty = new(ImmutableDictionary<ResourceSymbol, int>.Empty);

    private readonly ImmutableDictionary<ResourceSymbol, int> _amounts;

    public ResourceBundle(IEnumerable<KeyValuePair<ResourceSymbol, int>> amounts)
        : this(amounts.Where(kv => kv.Value != 0).ToImmutableDictionary())
    {
    }

    private ResourceBundle(ImmutableDictionary<ResourceSymbol, int> amounts)
    {
        _amounts = amounts;
    }

    public int this[ResourceSymbol symbol] => _amounts.TryGetValue(symbol, out var amount) ? amount : 0;

    public IReadOnlyDictionary<ResourceSymbol, int> Amounts => _amounts;

    public ResourceBundle With(ResourceSymbol symbol, int delta)
    {
        var newAmount = this[symbol] + delta;
        var updated = newAmount == 0 ? _amounts.Remove(symbol) : _amounts.SetItem(symbol, newAmount);
        return new ResourceBundle(updated);
    }

    public bool Equals(ResourceBundle? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_amounts.Count != other._amounts.Count) return false;
        foreach (var (symbol, amount) in _amounts)
        {
            if (!other._amounts.TryGetValue(symbol, out var otherAmount) || otherAmount != amount) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as ResourceBundle);

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var (symbol, amount) in _amounts) hash ^= HashCode.Combine(symbol, amount);
        return hash;
    }
}
