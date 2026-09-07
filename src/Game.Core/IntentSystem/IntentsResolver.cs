namespace MatchEngine;

public class IntentsResolver{

    private HashSet<Intent> _activeIntents;
    private HashSet<Intent> _queuedIntents;

    private EventBus _eventBus;
    public IntentsResolver(EventBus eventBus) //TODO: implement Decision => Intent logic.
    {
        _activeIntents = new();
        _queuedIntents = new();
        _eventBus = eventBus;

    }
    public void QueueIntent(Intent intent){
        _queuedIntents.Add(intent);
    }

    public bool RealizeIntents(){
        _activeIntents = _queuedIntents;
        _queuedIntents = new();
        if(_activeIntents.Count == 0) return false;
        foreach(var intent in _activeIntents){
            intent.Perform();
        }
        return true;
    }

    public void RealizeDecisions(TimeStamp timeStamp)
    {
        var collectionEvent = new CollectingIntentsPlanningPhase(timeStamp);
        _eventBus.Publish(collectionEvent);
        foreach(var intent in collectionEvent.Intents)
        {
            QueueIntent(intent);
        }
    }
}


public class CollectingIntentsPlanningPhase
{
    public TimeStamp TimeStamp; 
    public List<Intent> Intents = new();
    public CollectingIntentsPlanningPhase(TimeStamp timeStamp)
    {
        TimeStamp = timeStamp;
        Intents = new();
    }
}

