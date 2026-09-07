namespace MatchEngine;

public class SummoningResolver
{
    // Only ever needs MatchState, so it takes that directly rather than the whole MatchRoot.
    private readonly MatchState _matchState;

    public SummoningResolver(MatchState matchState)
    {
        _matchState = matchState;
    }

    // TODO: no summoning-request mechanism exists yet anywhere (nothing queues a summon), so
    // this always has nothing to do. Wire this up once summoning intents/decisions exist.
    public bool ProcessSummons()
    {
        return false;
    }
}
