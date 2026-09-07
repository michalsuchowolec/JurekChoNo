using MatchEngine;

public class IntModifierFactory(GameFlowManager gameFlowManager)
{
    public AddModifier Add(int value, bool isAura = true) =>
        new(value, gameFlowManager.Now, isAura);

    public MultModifier Mult(float factor, bool isAura = true) =>
        new(factor, gameFlowManager.Now, isAura);

    public DivModifier Div(float divisor, bool isAura = true) =>
        new(divisor, gameFlowManager.Now, isAura);

    public ClampMinModifier ClampMin(int min, bool isAura = true) =>
        new(min, gameFlowManager.Now, isAura);

    public ClampMaxModifier ClampMax(int max, bool isAura = true) =>
        new(max, gameFlowManager.Now, isAura);

    public ClampModifier Clamp(int min, int max, bool isAura = true) =>
        new(min, max, gameFlowManager.Now, isAura);
}
