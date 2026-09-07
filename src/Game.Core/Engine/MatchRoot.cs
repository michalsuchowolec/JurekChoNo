using Game.Core;

namespace MatchEngine;
public class MatchRoot{
    public EventBus Events { get; }
    public IdManager IdManager { get; }
    public CardFactory CardFactory { get; }
    public MatchState MatchState { get; }
    public EffectsTracker EffectsTracker { get; }
    public GameFlowManager GameFlowManager { get; }
    public ResolutionResolver ResolutionResolver { get; }

    // Order matters and is enforced here, not by whoever calls this: each step only depends on
    // what's already been assigned above it.
    //  - GameFlowManager subscribes to Events in its own constructor, so Events must exist first.
    //  - EffectsTracker needs MatchState to collect from.
    // playerData is what each player actually queued into the match (their starting deck) -
    // MatchRoot builds the real Player objects (and decks) from it, rather than receiving
    // pre-built ones. That also means player ids now come from the same IdManager as everything
    // else minted during the match - no more split id-space between setup and play.
    public MatchRoot(IReadOnlyList<InitialPlayerData> playerData)
    {
        Events = new EventBus();
        IdManager = new IdManager();
        CardFactory = new CardFactory();
        var players = BuildPlayers(playerData);
        MatchState = new MatchState(players);
        EffectsTracker = new EffectsTracker(MatchState);
        GameFlowManager = new GameFlowManager(this);
        ResolutionResolver = new ResolutionResolver();
    }

    // NOTE: Deck.Add still throws NotImplementedException (CardsContainer has no actual storage
    // yet), and CardFactory has no templates registered - so this only works today for empty
    // decks. Wiring real starting decks needs both of those built out first.
    private List<Player> BuildPlayers(IReadOnlyList<InitialPlayerData> playerData)
    {
        var players = new List<Player>();
        foreach(var data in playerData)
        {
            var player = new Player(IdManager.NextPlayerId());
            foreach(var template in data.DeckCardTemplates)
            {
                player.Deck.Add(CardFactory.CreateCardFromTemplate(template, IdManager));
            }
            players.Add(player);
        }
        return players;
    }
}
