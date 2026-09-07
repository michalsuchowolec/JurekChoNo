using Game.Core;

public class CardFactory
{
    public Card CreateCardFromTemplate(string template, IdManager idManager) 
    {
        var card = _cardTemplates[template]();
        card.Id = idManager.NextEntityId();
        return card;
    }   
    private Dictionary<string, Func<Card>> _cardTemplates = new(){
                
    };

}