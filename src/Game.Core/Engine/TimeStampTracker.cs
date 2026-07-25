namespace MatchEngine;

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
