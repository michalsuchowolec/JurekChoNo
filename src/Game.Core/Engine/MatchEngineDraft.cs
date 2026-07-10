using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Game.Core;

namespace MatchEngine;




public class GameFlowManager { //handles flow of the game, processes state changes
    //Upkeep
    //Play Decisions
    //Play Resolution
    //Movement Decisions
    //Movement Resolution
    //End of Turn

    //Data about state lives in GameState

}
public class PlayerDecisionsHandler{

}

public  class MomentResolver{ //handles a single automatic moment resolution
    private EffectsTracker _effectsTracker;
    private IntentsResolver _intentsResolver;
    private ZoneChangeResolver _zoneChangeResolver;
    private SummoningResolver _summoningResolver;
    private MovementResolver _movementResolver;
    private MatchState _matchState;
    private readonly GameEventSystem _events;

    private readonly TimeStampTracker _timeStampTracker;
    public GameEventSystem Events => _events;
    public EffectsTracker Effects => _effectsTracker;

    public IntentsResolver Intents => _intentsResolver;
    public TimeStamp Now => _timeStampTracker.Now;

    private HexPoseModifierFactory _hexPoseModifierFactory;
    public HexPoseModifierFactory HexPoseModifierFactory => _hexPoseModifierFactory;

    private StatEvaluator _statEvaluator;

    public StatEvaluator StatEvaluator => _statEvaluator;

    public MomentResolver(MatchState matchState){
        _timeStampTracker = new(); //TODO: Determine current timestamp based on gamestate
        _matchState = matchState;
        _events = new GameEventSystem();
        _statEvaluator = new StatEvaluator();
        _effectsTracker = new EffectsTracker(matchState);
        _intentsResolver = new IntentsResolver();
        _zoneChangeResolver = new ZoneChangeResolver();
        _summoningResolver = new SummoningResolver();
        _movementResolver = new MovementResolver(matchState, this);
        _hexPoseModifierFactory = new(_timeStampTracker);
    }

    public void ResolveMoment(){
        while(true){
            if(_effectsTracker.RealizeEffects()) continue;
            if(_intentsResolver.RealizeIntents(this)) continue;
            if(_zoneChangeResolver.ProcessZoneChanges(_matchState)) continue;
            if(_summoningResolver.ProcessSummons(_matchState)) continue;
            if(_movementResolver.ProcessMovement(_matchState)) continue;
            break;
        }
    }
}

public class TimeStampTracker
{
    private TimeStamp _currentTimeStamp;
    public TimeStamp Now => _currentTimeStamp;
    public void IncrementMoment()
    {
        _currentTimeStamp = new TimeStamp
        {
            Turn = _currentTimeStamp.Turn,
            Phase = _currentTimeStamp.Phase,
            Initiative = _currentTimeStamp.Initiative,
            Moment = _currentTimeStamp.Moment + 1
        };
    }

    public void IncrementInitiative()
    {
        _currentTimeStamp = new TimeStamp
        {
            Turn = _currentTimeStamp.Turn,
            Phase = _currentTimeStamp.Phase,
            Initiative = _currentTimeStamp.Initiative + 1,
            Moment = 0
        };
    }

    public void IncrementPhase()
    {
        _currentTimeStamp = new TimeStamp
        {
            Turn = _currentTimeStamp.Turn,
            Phase = _currentTimeStamp.Phase + 1,
            Initiative = 0,
            Moment = 0
        };
    }

    public void IncrementTurn()
    {
        _currentTimeStamp = new TimeStamp
        {
            Turn = _currentTimeStamp.Turn + 1,
            Phase = 0,
            Initiative = 0,
            Moment = 0
        };
    }
}


public class SummoningResolver
{
    public bool ProcessSummons(MatchState matchState)
    {
        throw new NotImplementedException();
    }
}

public class MovementResolver{

    private MatchState _matchState;
    private MomentResolver _momentResolver;
    private Dictionary<Unit, List<Modifier<HexPose>>> _plannedMoves = new();
    public MovementResolver(MatchState matchState, MomentResolver momentResolver)
    {
        _matchState = matchState;
        _momentResolver = momentResolver;
    }

    public bool ProcessMovement(MatchState matchState){
        throw new NotImplementedException();
    }

    public void RequestBasicMovement(Unit unit, HexDirection direction)
    {
        if (!_plannedMoves.ContainsKey(unit))
        {
            _plannedMoves[unit] = new();
        }   
        _plannedMoves[unit].Add(_momentResolver.HexPoseModifierFactory.RotateStatic((int) direction));
        _plannedMoves[unit].Add(_momentResolver.HexPoseModifierFactory.MoveRelative(HexPosition.UnitVector(direction)));
    }
}

public class ZoneChangeResolver{
    public bool ProcessZoneChanges(MatchState matchState){
        throw new NotFiniteNumberException();
    }
}

public class EffectsTracker{
    private HashSet<Effect> _activeEffects;
    private HashSet<Effect> _queuedEffects;
    public EffectsTracker(MatchState matchState){
        _activeEffects = FindExistingEffects(matchState);
        _queuedEffects = new();
    }

    private HashSet<Effect> FindExistingEffects(MatchState matchState)
    {
        throw new NotFiniteNumberException();
    }

    public void QueueEffect(Effect effect){
        _queuedEffects.Add(effect);
    }

    public bool RealizeEffects(){
        if(_queuedEffects.Count == 0) return false;
        foreach(var effect in _queuedEffects){
            _activeEffects.Add(effect);
            effect.Enable();
        }
        _queuedEffects.Clear();
        return true;
    }

    public void RemoveEffect(Effect effect){
        if(!_activeEffects.Contains(effect)) throw new Exception("Trying to remove unregistered effect");
        _activeEffects.Remove(effect);
        effect.Disable();
    }
}

public class IntentsResolver{

    private HashSet<Intent> _activeIntents;
    private HashSet<Intent> _queuedIntents;
    public IntentsResolver() //TODO: implement Decision => Intent logic.
    {
        _activeIntents = new();
        _queuedIntents = new();
    }
    public void QueueIntent(Intent intent){
        _queuedIntents.Add(intent);
    }

    public bool RealizeIntents(MomentResolver resolver){
        _activeIntents = _queuedIntents;
        _queuedIntents = new();
        if(_activeIntents.Count == 0) return false;
        foreach(var intent in _activeIntents){
            intent.Perform(resolver);
        }
        return true;
    }
}

public class GameEventSystem{
    public event Action<Intent>? CollectingIntentDamage;
    public event Action<Intent>? DamageDone;

    public void RaiseCollectIntentDamageModifiers(Intent intent){
        CollectingIntentDamage?.Invoke(intent);
    }
    public void RaiseDamageDone(Intent intent)
    {
        DamageDone?.Invoke(intent);
    }
}


public class StatEvaluator
{
    public event Action<Unit, List<Modifier<Health>>>? HealthModifiersRequested;
    public event Action<Unit, List<Modifier<int>>>? AttackModifiersRequested;
    public event Action<Unit, List<Modifier<int>>>? SpeedModifiersRequested;

    public List<Modifier<Health>> CollectHealthModifiers(Unit unit)
    {
        var modifiers = new List<Modifier<Health>>();
        HealthModifiersRequested?.Invoke(unit, modifiers);
        return modifiers;
    }

    public List<Modifier<int>> CollectAttackModifiers(Unit unit)
    {
        var modifiers = new List<Modifier<int>>();
        AttackModifiersRequested?.Invoke(unit, modifiers);
        return modifiers;
    }

    public List<Modifier<int>> CollectSpeedModifiers(Unit unit)
    {
        var modifiers = new List<Modifier<int>>();
        SpeedModifiersRequested?.Invoke(unit, modifiers);
        return modifiers;
    }
}