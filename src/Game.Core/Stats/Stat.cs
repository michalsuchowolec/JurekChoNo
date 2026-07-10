public abstract class Stat<T>
{
    public T BaseValue;

    protected Stat(T baseValue)
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
}
