namespace MatchEngine;

// What a player actually queues into a match (per the "decide locally, then queue" flow) - just
// their starting deck. MatchRoot builds the real Player/Card objects from this; it isn't handed
// pre-built ones. Deliberately doesn't carry test-only concerns (e.g. starting unit positions) -
// those go through a separate test-only path once one exists, not through this production shape.
public class InitialPlayerData
{
    public IReadOnlyList<string> DeckCardTemplates { get; }

    public InitialPlayerData(IReadOnlyList<string> deckCardTemplates)
    {
        DeckCardTemplates = deckCardTemplates;
    }
}
