public class Card
{
    public Stat<ResourceBundle> Cost;
    public bool CanBePlayed(CardPlayInput input)
    {
        throw new NotImplementedException();
    }
}

public class UnitCard : Card
{
    public static UnitCard FromUnit(Unit unit)
    {
        throw new NotImplementedException();
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

