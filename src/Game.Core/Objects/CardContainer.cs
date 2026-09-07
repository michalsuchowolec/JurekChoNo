namespace MatchEngine;

public enum CardsContainerPriority
{
    Hand,
    Deck,
    DiscardPile,
    PreparationZone
}

public abstract class CardsContainer : IComparable<CardsContainer>
{
    protected CardsContainer(Player owner)
    {
        Owner = owner;
    }

    public abstract void Add(Card card);
    public abstract void Remove(Card card);
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
    public DiscardPile(Player owner) : base(owner) { }
    public override CardsContainerPriority Priority => CardsContainerPriority.DiscardPile;
    public override void Add(Card card)
    {
        throw new NotFiniteNumberException();
    }

    public override void Remove(Card card)
    {
        throw new NotImplementedException();
    }

}

public class Hand : CardsContainer
{
    public Hand(Player owner) : base(owner) { }
    public override CardsContainerPriority Priority => CardsContainerPriority.Hand;
    public override void Add(Card card)
    {
        throw new NotImplementedException();
    }

    public override void Remove(Card card)
    {
        throw new NotImplementedException();
    }
}

public class Deck : CardsContainer
{
    public Deck(Player owner) : base(owner) { }
    public override CardsContainerPriority Priority => CardsContainerPriority.Deck;
    public override void Add(Card card)
    {
        throw new NotImplementedException();
    }

    public override void Remove(Card card)
    {
        throw new NotImplementedException();
    }
}


public class PreparationZone : CardsContainer
{
    public PreparationZone(Player owner) : base(owner){}
    public override CardsContainerPriority Priority => CardsContainerPriority.PreparationZone;

    public override void Add(Card card)
    {
        throw new NotImplementedException();
    }

    public override void Remove(Card card)
    {
        throw new NotImplementedException();
    }
}
