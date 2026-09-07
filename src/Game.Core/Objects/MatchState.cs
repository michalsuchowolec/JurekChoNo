using MatchEngine;
public class MatchState{
    public IReadOnlyList<Player> Players { get; }
    public Board Board { get; }

    public MatchState(IReadOnlyList<Player> players)
    {
        Players = players;
        Board = new Board(HexArea.Circle(7));
    }
}
