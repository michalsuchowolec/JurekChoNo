public class Stat<T>
{
    public T BaseValue;

    public Stat(T baseValue)
    {
        BaseValue = baseValue;
    }

    public T Evaluate(List<Modifier<T>> modifiers)
    {
        modifiers.Sort();
        var value = BaseValue;
        foreach (var modifier in modifiers)
            value = modifier.Transform(value);
        return value;
    }

    public static T Evaluate(T initialValue, List<Modifier<T>> modifiers)
    {
        modifiers.Sort();
        var value = initialValue;
        foreach (var modifier in modifiers)
            value = modifier.Transform(value);
        return value;
    }
}
