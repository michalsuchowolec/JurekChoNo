using System.Reflection.Metadata.Ecma335;
using Game.Core;

public class MatchRoot{
    public MatchState MatchState;
    public MatchEngine MatchEngine;

}

public class TurnResolver{

}


public class MatchState{
    

}

public class MatchEngine{
    public TurnResolver TurnResolver = new();
    public StatsEvaluator StatsEvaluator = new();
    public EventsSystem Events = new();
}


public class StatsEvaluator{
    public event Action<Unit, List<Modifier<int>>>? CollectingHealthModifiers;
    public int CalculateHealth(Unit unit){
        List<Modifier<int>> modifiers = new();
        CollectingHealthModifiers?.Invoke(unit, modifiers);
        modifiers.Sort();
        return unit.Health.Evaluate(modifiers);
    }


}


public class EventsSystem{
    public event Action<int> TurnEnded;


}










public enum StatOwner{
    Unit,
    Tile,
    Intent,
    Effect
}









public struct TimeStamp : IComparable<TimeStamp>{
    public int Turn;
    public int Phase;
    public int Initiative;
    public int Moment;
    public int CompareTo(TimeStamp other)
    {
        int c = Turn.CompareTo(other.Turn);
        if (c != 0) return c;
        c = Phase.CompareTo(other.Phase);
        if (c != 0) return c;
        c = Initiative.CompareTo(other.Initiative);
        if (c != 0) return c;
        return Moment.CompareTo(other.Moment);
    }
}

