using MatchEngine;

public class IntModifierFactory(TimeStampTracker timeStampTracker)
{
    public AddModifier Add(int value, bool isAura = true) =>
        new(value, timeStampTracker.Now, isAura);

    public MultModifier Mult(float factor, bool isAura = true) =>
        new(factor, timeStampTracker.Now, isAura);

    public DivModifier Div(float divisor, bool isAura = true) =>
        new(divisor, timeStampTracker.Now, isAura);

    public ClampMinModifier ClampMin(int min, bool isAura = true) =>
        new(min, timeStampTracker.Now, isAura);

    public ClampMaxModifier ClampMax(int max, bool isAura = true) =>
        new(max, timeStampTracker.Now, isAura);

    public ClampModifier Clamp(int min, int max, bool isAura = true) =>
        new(min, max, timeStampTracker.Now, isAura);
}
