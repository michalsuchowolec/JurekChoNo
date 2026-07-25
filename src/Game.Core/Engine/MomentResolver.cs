namespace MatchEngine;
public  class MomentResolver{ //handles a single automatic moment resolution
    private EffectsTracker _effectsTracker;
    private IntentsResolver _intentsResolver;
    private ZoneChangeResolver _zoneChangeResolver;
    private SummoningResolver _summoningResolver;
    private MovementResolver _movementResolver;
    private MatchState _matchState;
    private readonly EventBus _events;

    private readonly TimeStampTracker _timeStampTracker;
    public TimeStampTracker TimeStampTracker => _timeStampTracker;
    public EventBus Events => _events;
    public EffectsTracker Effects => _effectsTracker;

    public IntentsResolver Intents => _intentsResolver;
    public TimeStamp Now => _timeStampTracker.Now;

    private HexPoseModifierFactory _hexPoseModifierFactory;
    public HexPoseModifierFactory HexPoseModifierFactory => _hexPoseModifierFactory;

    public MomentResolver(MatchState matchState){
        _timeStampTracker = new(); //TODO: Determine current timestamp based on gamestate
        _matchState = matchState;
        _events = new EventBus();
        _effectsTracker = new EffectsTracker(matchState);
        _intentsResolver = new IntentsResolver();
        _zoneChangeResolver = new ZoneChangeResolver(matchState.Board, _events);
        _summoningResolver = new SummoningResolver();
        _hexPoseModifierFactory = new(_timeStampTracker);
        _movementResolver = new MovementResolver(matchState.Board, _hexPoseModifierFactory);
    }

    public void ResolveMoment(){
        while(true){
            if(_effectsTracker.RealizeEffects()) continue;
            if(_intentsResolver.RealizeIntents()) continue;
            if(_zoneChangeResolver.ProcessZoneChanges()) continue;
            if(_summoningResolver.ProcessSummons(_matchState)) continue;
            if(_movementResolver.ProcessMovement()) continue;
            break;
        }
    }
}
