namespace MatchEngine;

public class IntentsResolver{

    private HashSet<Intent> _activeIntents;
    private HashSet<Intent> _queuedIntents;
    public IntentsResolver() //TODO: implement Decision => Intent logic.
    {
        _activeIntents = new();
        _queuedIntents = new();
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
}
