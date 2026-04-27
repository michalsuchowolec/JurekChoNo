public class Stat<T>{
    public T BaseValue;
    public T Evaluate(List<Modifier<T>> modifiers){
        var value = BaseValue;
        foreach(var modifier in modifiers){
            value = modifier.Transform(value);
        }
        return value;
    }
    public Stat(T baseValue){
        BaseValue = baseValue;
    }
}