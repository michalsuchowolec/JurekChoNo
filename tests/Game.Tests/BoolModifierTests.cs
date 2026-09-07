using FluentAssertions;

namespace Game.Tests;

public class BoolModifierTests
{
    private static TimeStamp At(int turn) => new() { Turn = turn, Phase = 0, Initiative = 1, Moment = 0 };

    // Auras always beat non-auras, regardless of grant order or direction - IsAura is still the
    // primary sort key.
    [Fact]
    public void OlderAura_BeatsNewerNonAura_RegardlessOfDirection()
    {
        var modifiers = new List<Modifier<bool>>
        {
            new SetFalseModifier(At(0), isAura: true),   // an old, persistent "can't move" zone
            new SetTrueModifier(At(5), isAura: false),   // a fresh, one-shot "you can move" grant
        };

        Stat<bool>.Evaluate(true, modifiers).Should().BeFalse();
    }

    // Auras ignore recency entirely (ModifierPriority.CompareTo skips TimeStamp once both sides
    // are auras) and are ordered by OperationPriority instead, where SetTrue always outranks
    // SetFalse. So between two auras, SetTrue wins no matter which was granted first - this is
    // what "can always move, regardless of other effects" actually means: describe it as an aura,
    // and it beats any SetFalse aura unconditionally.
    [Fact]
    public void BetweenTwoAuras_SetTrueAlwaysWins_GrantedFirst()
    {
        var modifiers = new List<Modifier<bool>>
        {
            new SetTrueModifier(At(0), isAura: true),
            new SetFalseModifier(At(5), isAura: true),
        };

        Stat<bool>.Evaluate(true, modifiers).Should().BeTrue();
    }

    [Fact]
    public void BetweenTwoAuras_SetTrueAlwaysWins_GrantedLast()
    {
        var modifiers = new List<Modifier<bool>>
        {
            new SetFalseModifier(At(0), isAura: true),
            new SetTrueModifier(At(5), isAura: true),
        };

        Stat<bool>.Evaluate(true, modifiers).Should().BeTrue();
    }

    // The opposite rule for one-time (non-aura) changes: TimeStamp still decides, no
    // OperationPriority override - matching how e.g. two damage instances apply in the order they
    // actually happened.
    [Fact]
    public void BetweenTwoOneTimeChanges_MostRecentWins_RegardlessOfDirection()
    {
        var modifiers = new List<Modifier<bool>>
        {
            new SetTrueModifier(At(0), isAura: false),
            new SetFalseModifier(At(5), isAura: false),
        };

        Stat<bool>.Evaluate(true, modifiers).Should().BeFalse();
    }

    [Fact]
    public void NoModifiers_ReturnsBaseValue()
    {
        Stat<bool>.Evaluate(true, new List<Modifier<bool>>()).Should().BeTrue();
    }
}
