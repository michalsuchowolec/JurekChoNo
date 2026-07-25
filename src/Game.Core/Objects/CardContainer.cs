namespace MatchEngine;

public enum CardsContainerPriority
{
    Hand,
    Deck,
    DiscardPile
}

public abstract class CardsContainer : IComparable<CardsContainer>
{
    public abstract void Add(Card card);
    public Player Owner{get;}
    public abstract CardsContainerPriority Priority { get; }

    public int CompareTo(CardsContainer? other) => Priority.CompareTo(other!.Priority);

    public static bool HasPriority(CardsContainer candidate, CardsContainer current, Player owner)
    {
        int comparison = candidate.CompareTo(current);
        if(comparison != 0) return comparison < 0;

        bool candidateIsOwner = candidate.Owner == owner;
        bool currentIsOwner = current.Owner == owner;
        if(candidateIsOwner != currentIsOwner) return candidateIsOwner;

        return true;
        //same zone tier, neither (or both) the owner's container: whichever sets it wins, no seeded tiebreak.
    }
}
public class DiscardPile : CardsContainer
{
    public override CardsContainerPriority Priority => CardsContainerPriority.DiscardPile;
    public override void Add(Card card)
    {
        throw new NotFiniteNumberException();
    }
}

public class Hand : CardsContainer
{
    public override CardsContainerPriority Priority => CardsContainerPriority.Hand;
    public override void Add(Card card)
    {
        throw new NotImplementedException();
    }

}

public class Deck : CardsContainer
{
    public override CardsContainerPriority Priority => CardsContainerPriority.Deck;
    public override void Add(Card card)
    {
        throw new NotImplementedException();
    }

}
