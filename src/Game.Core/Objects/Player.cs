using Game.Core;
using MatchEngine;

public class Player
{
    public readonly PlayerId Id;
    public DiscardPile DiscardPile {get;}
    public Hand Hand{get;}
    public Deck Deck{get;}
    public PreparationZone PreparationZone{get;}

    // Whether this player has signalled they're done acting for the current decision-collecting
    // phase (Play/Movement) - GameFlowManager reads this to gate phase advancement and resets it
    // on every phase change. Lives on Player itself rather than an external tracker: it's this
    // player's own state, nobody else's.
    public bool IsReady { get; private set; }
    public void MarkReady() => IsReady = true;
    public void MarkNotReady() => IsReady = false;

    public Player(PlayerId id)
    {
        Id = id;
        DiscardPile = new DiscardPile(this);
        Hand = new Hand(this);
        Deck = new(this);
        PreparationZone = new(this);
    }
}