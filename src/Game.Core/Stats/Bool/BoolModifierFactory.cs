using MatchEngine;

public class BoolModifierFactory(GameFlowManager gameFlowManager)
{
    public SetFalseModifier SetFalse(bool isAura = true) =>
        new(gameFlowManager.Now, isAura);

    public SetTrueModifier SetTrue(bool isAura = true) =>
        new(gameFlowManager.Now, isAura);
}
