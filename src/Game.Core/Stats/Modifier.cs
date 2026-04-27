public abstract class Modifier<T> : IComparable<Modifier<T>>
{
    public ModifierPriority Priority { get; init; }
    public int CompareTo(Modifier<T>? other)
    {
        return Priority.CompareTo(other!.Priority);
    }
    public abstract T Transform(T initialState);
    
}


public struct ModifierPriority{
    public TimeStamp TimeStamp;
    public bool IsAura;
    public int OperationPriority;
    public long ValueSortKey;

    public ModifierPriority(TimeStamp timeStamp, bool isAura, int operationPriority, long valueSortKey = 0){
        TimeStamp = timeStamp;
        IsAura = isAura;
        OperationPriority = operationPriority;
        ValueSortKey = valueSortKey;
    }

    public static long PackIntPair(int a, int b) => ((long)a << 32) | (uint)b;

    public int CompareTo(ModifierPriority other)
    {
        int cmp = IsAura.CompareTo(other.IsAura);
        if (cmp != 0) return cmp;
        cmp = TimeStamp.CompareTo(other.TimeStamp);
        if (cmp != 0) return cmp;
        cmp = OperationPriority.CompareTo(other.OperationPriority);
        if (cmp != 0) return cmp;
        return ValueSortKey.CompareTo(other.ValueSortKey);
    }
}


