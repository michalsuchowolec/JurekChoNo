namespace MatchEngine;
public class MomentResolver{ //handles a single automatic moment resolution
    // The one thing MomentResolver holds onto directly - everything match-lifetime (Events,
    // EffectsTracker, GameFlowManager, MatchState, ...) is reached through this instead of being
    // copied into its own field, same idiom GameFlowManager uses for _matchRoot.
    private readonly MatchRoot _matchRoot;
    public MatchRoot MatchRoot => _matchRoot;

    private readonly IntentsResolver _intentsResolver;
    private readonly ZoneChangeResolver _zoneChangeResolver;
    private readonly SummoningResolver _summoningResolver;
    private readonly MovementResolver _movementResolver;

    public GameFlowManager GameFlowManager => _matchRoot.GameFlowManager;
    public EventBus Events => _matchRoot.Events;
    public EffectsTracker Effects => _matchRoot.EffectsTracker;
    public IntentsResolver Intents => _intentsResolver;
    public TimeStamp Now => _matchRoot.GameFlowManager.Now;

    private readonly HexPoseModifierFactory _hexPoseModifierFactory;
    public HexPoseModifierFactory HexPoseModifierFactory => _hexPoseModifierFactory;
    public MovementResolver MovementResolver => _movementResolver;

    // Private: a MomentResolver only exists for the lifetime of a single ResolveMoment call.
    private MomentResolver(MatchRoot matchRoot){
        _matchRoot = matchRoot;

        // These are scoped to a single moment resolution, so MomentResolver still owns and creates
        // them itself. Each one only needs a single thing (MatchRoot, or - for the two that need
        // more than one - this MomentResolver itself), rather than being handed its dependencies
        // piecemeal.
        _intentsResolver = new IntentsResolver(_matchRoot.Events);
        _zoneChangeResolver = new ZoneChangeResolver(_matchRoot);
        _summoningResolver = new SummoningResolver(_matchRoot.MatchState);
        _hexPoseModifierFactory = new HexPoseModifierFactory(_matchRoot.GameFlowManager);
        _movementResolver = new MovementResolver(this);
    }

    // The one public entry point: creates a fresh resolver, gives the caller a chance to seed it
    // with intents (built against `Events`/`MovementResolver`, which only exist once this resolver
    // does - see INPUT_SYSTEM.md's "IntentFactory" note), then drains the moment to completion.
    // Nothing currently supplies a seed - the old central PlayerDecisionsHandler that used to is
    // gone, and its decentralized replacement (see INPUT_SYSTEM.md) isn't wired in yet.
    public static void ResolveMoment(MatchRoot matchRoot, Action<MomentResolver>? seed = null){
        var resolver = new MomentResolver(matchRoot);
        seed?.Invoke(resolver);
        resolver.Drain();
    }

    private void Drain(){
        while(true){
            if(Effects.RealizeEffects()) continue;
            if(_intentsResolver.RealizeIntents()) continue;
            if(_zoneChangeResolver.ProcessZoneChanges()) continue;
            if(_summoningResolver.ProcessSummons()) continue;

            if(_movementResolver.ProcessMovement()) continue;
            break;
        }
    }
}
