namespace MatchEngine;

public class GameFlowManager { //handles flow of the game: match lifecycle and TimeStamp progression

    private const int PhaseCount = 6; // Upkeep, Play, PlayResolution, Movement, MovementResolution, EndStep
    public const int MovementInitiativeCount = 3; // MovementResolution is the only phase with more than one initiative slot
    private TimeStamp _timeStamp;
    public TimeStamp Now => _timeStamp;
    public TurnPhase TurnPhase => (TurnPhase)_timeStamp.Phase;

    private MatchStatus _status = MatchStatus.NotStarted;
    public MatchStatus Status => _status;

    private readonly MatchRoot _matchRoot;

    // Not a snapshot: always the live bus on _matchRoot, so nothing here goes stale if it's ever
    // swapped. Doesn't remove the ordering requirement below though - Subscribe is an eager call
    // made once, during construction, so matchRoot.Events must already exist at that point either way.
    private EventBus Events => _matchRoot.Events;

    // GameFlowManager doesn't decide when a match starts or ends - it reacts to
    // MatchStartRequested/MatchEndRequested events published by whatever does (a lobby once both
    // players are in, a resignation, a win-condition check, ...), and owns the resulting state
    // transition and first/last moment resolution.
    // Requires matchRoot.Events to already be set (this subscribes against it immediately).
    public GameFlowManager(MatchRoot matchRoot)
    {
        _matchRoot = matchRoot;
        Events.Subscribe<MatchStartRequested>(OnMatchStartRequested);
        Events.Subscribe<MatchEndRequested>(OnMatchEndRequested);
    }

    private void OnMatchStartRequested(MatchStartRequested request)
    {
        if(_status != MatchStatus.NotStarted) return;

        _status = MatchStatus.InProgress;
        Events.Publish(new MatchStarted());
        // TimeStamp already defaults to Turn 0 / Upkeep / Initiative 1 - just resolve that first moment.
        MomentResolver.ResolveMoment(_matchRoot);
    }

    private void OnMatchEndRequested(MatchEndRequested request)
    {
        if(_status != MatchStatus.InProgress) return;

        _status = MatchStatus.Ended;
        Events.Publish(new MatchEnded());
    }

    // Play and Movement are where players submit decisions; they can only end once every
    // player has signalled they're done (ends up as "end play phase" / "end move phase" input).
    public bool MoveToNextPhase()
    {
        if(_status != MatchStatus.InProgress) return false;
        if(RequiresAllPlayersReady(TurnPhase) && !_matchRoot.MatchState.Players.All(player => player.IsReady)) return false;

        Advance();
        foreach(var player in _matchRoot.MatchState.Players) player.MarkNotReady();
        Events.Publish(new TurnPhaseChanged(TurnPhase));
        MomentResolver.ResolveMoment(_matchRoot);
        return true;
    }

    public void MarkPlayerReady(Player player) => player.MarkReady();

    private static bool RequiresAllPlayersReady(TurnPhase phase) =>
        phase == TurnPhase.Play || phase == TurnPhase.Movement;

    // All phases occupy a single initiative slot (Initiative 1) and simply move to the next
    // phase. MovementResolution is the exception: initiative counts down from
    // MovementInitiativeCount to 1 - a unit with Speed N can act in initiatives N, N-1, ..., 1,
    // so "can this unit act at the current initiative" is just currentInitiative <= unit.Speed,
    // no maxInitiative - currentInitiative arithmetic needed anywhere.
    private void Advance()
    {
        if(TurnPhase == TurnPhase.MovementResolution && _timeStamp.Initiative > 1)
        {
            DecrementInitiative();
        }
        else
        {
            IncrementPhase();
        }
    }

    private void IncrementMoment()
    {
        _timeStamp = new TimeStamp
        {
            Turn = _timeStamp.Turn,
            Phase = _timeStamp.Phase,
            Initiative = _timeStamp.Initiative,
            Moment = _timeStamp.Moment + 1
        };
    }

    private void DecrementInitiative()
    {
        _timeStamp = new TimeStamp
        {
            Turn = _timeStamp.Turn,
            Phase = _timeStamp.Phase,
            Initiative = _timeStamp.Initiative - 1,
            Moment = 0
        };
    }

    private void IncrementPhase()
    {
        var nextPhase = _timeStamp.Phase + 1;
        if(nextPhase >= PhaseCount)
        {
            IncrementTurn();
            return;
        }
        _timeStamp = new TimeStamp
        {
            Turn = _timeStamp.Turn,
            Phase = nextPhase,
            // MovementResolution starts its countdown at MovementInitiativeCount; every other
            // phase has just the one slot, Initiative 1.
            Initiative = (TurnPhase)nextPhase == TurnPhase.MovementResolution ? MovementInitiativeCount : 1,
            Moment = 0
        };
    }

    private void IncrementTurn()
    {
        _timeStamp = new TimeStamp
        {
            Turn = _timeStamp.Turn + 1,
            Phase = 0,
            Initiative = 1,
            Moment = 0
        };
    }
}

public enum MatchStatus
{
    NotStarted,
    InProgress,
    Ended
}

// Trigger events: published by whatever decides a match should begin/end.
public record MatchStartRequested;
public record MatchEndRequested;

// Fact events: published by GameFlowManager once it has actually processed the request.
public record MatchStarted;
public record MatchEnded;

public record TurnPhaseChanged(TurnPhase TurnPhase);

public enum TurnPhase
{
    Upkeep,
    Play,
    PlayResolution,
    Movement,
    MovementResolution,
    EndStep
}
