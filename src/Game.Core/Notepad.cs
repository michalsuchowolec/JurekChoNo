using System.ComponentModel;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using Game.Core;
using MatchEngine;

public class MatchRoot{
    public MatchState MatchState;
    public MomentResolver? MomentResolver;

}



public class MatchState{
    

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

