using Game.Core;
using MatchEngine;

public class Card : IEffectsOwner
{
    public EntityId Id{get;set;}
    public Stat<ResourceBundle> Cost;
    private List<Effect> _effects = new();
    public IEnumerable<Effect> Effects => _effects;
    public bool CanBePlayed(CardPlayInput input)
    {
        throw new NotImplementedException();
    }
    public void AddEffect(Effect effect)
    {
        _effects.Add(effect);
    }
}


public class UnitConvertedToCard
{
    public Unit Unit;
    public List<Effect> Effects;
}
public class UnitCard : Card
{
    public static Card FromUnit(Unit unit, MatchRoot matchRoot)
    {
        var conversionEvent = new UnitConvertedToCard
        {
            Unit = unit,
            Effects = new()
        };
        var card = matchRoot.CardFactory.CreateCardFromTemplate(unit.Template, matchRoot.IdManager);
        matchRoot.Events.Publish(conversionEvent);
        foreach(var effect in conversionEvent.Effects)
        {
            matchRoot.EffectsTracker.QueueEffect(effect, card);
        }
        return card;
    }
}

public class EmanationCard : Card
{
    
}
public class InvocationCard : Card
{
    
}

public class CardPlayInput
{
    
}

