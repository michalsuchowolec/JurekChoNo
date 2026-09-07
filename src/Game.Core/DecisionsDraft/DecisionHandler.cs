using System.Xml;
using Game.Core;
using Microsoft.VisualBasic;

namespace MatchEngine;

public abstract class DecisionHandler
{
    private DecisionStep _decisionStep;
    public DecisionHandler(EventBus events)
    {
        Clear();
        events.Subscribe<CollectingIntentsPlanningPhase>(IntentCollectionCallback);
    }
    private void IntentCollectionCallback(CollectingIntentsPlanningPhase collectionEvent)
    {
        var intent = GenerateIntent();
        if(intent != null) collectionEvent.Intents.Add(intent);
    }
    abstract protected Intent? GenerateIntent();
    abstract public void Clear();
}

public class PlayerDecision
{
    
}

public class UnitDecisionHandler
{
    private Unit _unit;
    private Intent?[] _intents = new Intent[3];
    public void  InitializeDecision()
    {
        
    } 
}

public class Decision
{
    public Stack<DecisionStep> Steps;

}

public abstract class DecisionStep
{

    public DecisionStepType Type = DecisionStepType.PlainCommit;
    public DecisionHandler Owner;
    public Decision Decision;
    
}



public enum DecisionStepType
{
    PlainCommit
}






public enum UnitAction
{
    Move,
    Rotate,
    Wait,
    UseInvocation,
    UseAbility
}


