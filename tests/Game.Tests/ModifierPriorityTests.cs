using FluentAssertions;

namespace Game.Tests;

public class ModifierPriorityTests
{
    private static TimeStamp At(int turn) => new() { Turn = turn, Phase = 0, Initiative = 1, Moment = 0 };

    // Deliberately set up so timestamp-based and OperationPriority-based ordering disagree, to
    // prove it's actually OperationPriority deciding for auras, not TimeStamp.
    [Fact]
    public void Auras_AreOrderedByOperationPriority_EvenWhenThatContradictsTimestamp()
    {
        var oldButLowPriority = new ModifierPriority(At(0), isAura: true, operationPriority: 9);
        var newButHighPriority = new ModifierPriority(At(100), isAura: true, operationPriority: 1);

        newButHighPriority.CompareTo(oldButLowPriority).Should().BeLessThan(0);
    }

    [Fact]
    public void Auras_WithEqualOperationPriority_CompareEqual_RegardlessOfTimestamp()
    {
        var early = new ModifierPriority(At(0), isAura: true, operationPriority: 3);
        var late = new ModifierPriority(At(500), isAura: true, operationPriority: 3);

        early.CompareTo(late).Should().Be(0);
    }

    // The opposite rule for one-time changes: TimeStamp still decides, even when that contradicts
    // OperationPriority - recency genuinely matters for something like damage instances.
    [Fact]
    public void OneTimeChanges_AreStillOrderedByTimestamp_EvenWhenThatContradictsOperationPriority()
    {
        var earlier = new ModifierPriority(At(0), isAura: false, operationPriority: 9);
        var later = new ModifierPriority(At(1), isAura: false, operationPriority: 1);

        earlier.CompareTo(later).Should().BeLessThan(0);
    }

    [Fact]
    public void Aura_OutranksNonAura_RegardlessOfTimestampOrOperationPriority()
    {
        var aura = new ModifierPriority(At(0), isAura: true, operationPriority: 0);
        var oneTime = new ModifierPriority(At(1000), isAura: false, operationPriority: 99);

        aura.CompareTo(oneTime).Should().BeGreaterThan(0);
    }
}
