namespace MatchEngine;

public class EffectsTracker{
    private HashSet<Effect> _activeEffects;
    private HashSet<Effect> _queuedEffects;
    public EffectsTracker(MatchState matchState){
        _activeEffects = CollectEffects(matchState);
        _queuedEffects = new();
    }

    private HashSet<Effect> CollectEffects(MatchState matchState)
    {
        throw new NotFiniteNumberException();
    }

    public void QueueEffect(Effect effect){
        _queuedEffects.Add(effect);
    }

    public bool RealizeEffects(){
        if(_queuedEffects.Count == 0) return false;
        foreach(var effect in _queuedEffects){
            _activeEffects.Add(effect);
            effect.Enable();
        }
        _queuedEffects.Clear();
        return true;
    }

    public void RemoveEffect(Effect effect){
        if(!_activeEffects.Contains(effect)) throw new Exception("Trying to remove unregistered effect");
        _activeEffects.Remove(effect);
        effect.Disable();
    }
}
