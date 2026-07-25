namespace MatchEngine;

public class EventBus{
    private readonly Dictionary<Type, Delegate> _handlers = new();

    public void Subscribe<T>(Action<T> handler){
        _handlers[typeof(T)] = _handlers.TryGetValue(typeof(T), out var existing)
            ? Delegate.Combine(existing, handler)
            : handler;
    }

    public void Unsubscribe<T>(Action<T> handler){
        if(!_handlers.TryGetValue(typeof(T), out var existing)) return;
        var result = Delegate.Remove(existing, handler);
        if(result == null) _handlers.Remove(typeof(T));
        else _handlers[typeof(T)] = result;
    }

    public void Publish<T>(T evt){
        if(_handlers.TryGetValue(typeof(T), out var handler)) ((Action<T>)handler)?.Invoke(evt);
    }
}
