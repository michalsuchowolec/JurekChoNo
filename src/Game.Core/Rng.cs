namespace Game.Core;

/// <summary>
/// Seed value for deterministic random number generation.
/// </summary>
public readonly record struct GameSeed(int Value)
{
    public static GameSeed FromInt(int value) => new(value);
    public override string ToString() => $"Seed({Value})";
}

/// <summary>
/// Abstraction for random number generation, enabling deterministic replays.
/// </summary>
public interface IRng
{
    /// <summary>Returns a non-negative random integer.</summary>
    int Next();

    /// <summary>Returns a random integer in [0, maxExclusive).</summary>
    int Next(int maxExclusive);

    /// <summary>Returns a random integer in [minInclusive, maxExclusive).</summary>
    int Next(int minInclusive, int maxExclusive);

    /// <summary>Returns a random double in [0.0, 1.0).</summary>
    double NextDouble();

    /// <summary>Shuffles the list in place using Fisher-Yates.</summary>
    void Shuffle<T>(IList<T> list);

    /// <summary>Creates a fork with a derived seed (for isolated sub-sequences).</summary>
    IRng Fork();
}

/// <summary>
/// Deterministic RNG implementation using a linear congruential generator.
/// Portable and produces identical sequences across platforms.
/// </summary>
public sealed class DeterministicRng : IRng
{
    private const long Multiplier = 6364136223846793005L;
    private const long Increment = 1442695040888963407L;

    private ulong _state;

    public DeterministicRng(GameSeed seed)
    {
        _state = (ulong)seed.Value;
        _state = NextState();
    }

    private DeterministicRng(ulong state)
    {
        _state = state;
    }

    private ulong NextState()
    {
        _state = unchecked(_state * Multiplier + Increment);
        return _state;
    }

    public int Next()
    {
        return (int)(NextState() >> 33);
    }

    public int Next(int maxExclusive)
    {
        if (maxExclusive <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxExclusive));
        return (int)((uint)Next() % (uint)maxExclusive);
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
            throw new ArgumentOutOfRangeException(nameof(maxExclusive));
        return minInclusive + Next(maxExclusive - minInclusive);
    }

    public double NextDouble()
    {
        return (NextState() >> 11) * (1.0 / (1UL << 53));
    }

    public void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public IRng Fork()
    {
        ulong childState = NextState();
        ulong parentAdvance = NextState();
        _ = parentAdvance; // parent state already advanced
        return new DeterministicRng(childState);
    }
}
