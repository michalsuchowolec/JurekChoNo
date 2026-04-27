using FluentAssertions;
using Game.Core;

namespace Game.Tests;

public class HexPosTests
{
    // ── Distance ─────────────────────────────────────────────────────────────

    [Fact]
    public void Distance_SamePos_ReturnsZero()
    {
        var pos = new HexPos(3, -2);
        HexPos.Distance(pos, pos).Should().Be(0);
    }

    [Fact]
    public void Distance_AllNeighbours_ReturnsOne()
    {
        var origin = HexPos.Origin;
        foreach (var neighbour in origin.Neighbors())
            origin.DistanceTo(neighbour).Should().Be(1);
    }

    [Theory]
    //              x1  y1  x2  y2  dist
    [InlineData(0,  0,  3,  0,  3)]   // straight UpRight
    [InlineData(0,  0,  0,  3,  3)]   // straight Up
    [InlineData(0,  0, -3,  3,  3)]   // straight UpLeft
    [InlineData(0,  0,  2, -1,  2)]   // DownRight x2
    [InlineData(1,  2,  4, -1,  3)]   // non-origin
    public void Distance_VariousPositions(int x1, int y1, int x2, int y2, int expected)
    {
        var a = new HexPos(x1, y1);
        var b = new HexPos(x2, y2);
        HexPos.Distance(a, b).Should().Be(expected);
        a.DistanceTo(b).Should().Be(expected);
    }

    // ── Neighbours ───────────────────────────────────────────────────────────

    [Fact]
    public void Neighbors_ReturnsSixUniqueHexes()
    {
        var pos = new HexPos(2, -1);
        var neighbours = pos.Neighbors().ToList();
        neighbours.Should().HaveCount(6);
        neighbours.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Neighbors_AllAtDistanceOne()
    {
        var pos = HexPos.Origin;
        foreach (var n in pos.Neighbors())
            pos.DistanceTo(n).Should().Be(1);
    }

    // ── Arithmetic ───────────────────────────────────────────────────────────

    [Fact]
    public void Addition_CombinesCoordinates()
    {
        var result = new HexPos(1, 2) + new HexPos(3, -1);
        result.Should().Be(new HexPos(4, 1));
    }

    [Fact]
    public void Subtraction_SubtractsCoordinates()
    {
        var result = new HexPos(5, 3) - new HexPos(2, 1);
        result.Should().Be(new HexPos(3, 2));
    }

    // ── Direction offsets ────────────────────────────────────────────────────

    [Theory]
    [InlineData(HexDirection.Up,        0,  1)]
    [InlineData(HexDirection.UpRight,   1,  0)]
    [InlineData(HexDirection.DownRight, 1, -1)]
    [InlineData(HexDirection.Down,      0, -1)]
    [InlineData(HexDirection.DownLeft, -1,  0)]
    [InlineData(HexDirection.UpLeft,   -1,  1)]
    public void Direction_Offset_MatchesExpected(HexDirection dir, int ex, int ey)
    {
        dir.ToOffset().Should().Be(new HexPos(ex, ey));
    }

    [Theory]
    [InlineData(HexDirection.Up,        HexDirection.Down)]
    [InlineData(HexDirection.UpRight,   HexDirection.DownLeft)]
    [InlineData(HexDirection.DownRight, HexDirection.UpLeft)]
    public void Direction_Opposite_IsCorrect(HexDirection dir, HexDirection expected)
    {
        dir.Opposite().Should().Be(expected);
        expected.Opposite().Should().Be(dir);
    }

    // ── Rotation (position) ──────────────────────────────────────────────────

    [Fact]
    public void Rotate_CW_CyclesDirectionOffsets()
    {
        // Each direction offset should rotate into the next CW direction offset.
        var dirs = HexDirectionExt.All;
        for (int i = 0; i < dirs.Length; i++)
        {
            var current = dirs[i].ToOffset();
            var nextCW  = dirs[(i + 1) % dirs.Length].ToOffset();
            current.RotateCW().Should().Be(nextCW);
        }
    }

    [Fact]
    public void Rotate_CCW_CyclesDirectionOffsets()
    {
        var dirs = HexDirectionExt.All;
        for (int i = 0; i < dirs.Length; i++)
        {
            var current = dirs[i].ToOffset();
            var nextCCW = dirs[(i + 5) % dirs.Length].ToOffset();
            current.RotateCCW().Should().Be(nextCCW);
        }
    }

    [Fact]
    public void Rotate_SixSteps_IsIdentity()
    {
        var pos = new HexPos(3, -1);
        pos.Rotate(6).Should().Be(pos);
        pos.Rotate(-6).Should().Be(pos);
    }

    [Fact]
    public void Rotate_NegativeSteps_EquivalentToCCW()
    {
        var pos = new HexPos(2, 1);
        pos.Rotate(-1).Should().Be(pos.RotateCCW());
        pos.Rotate(-2).Should().Be(pos.RotateCCW().RotateCCW());
    }
}

public class HexTransformTests
{
    // ── Rotate ───────────────────────────────────────────────────────────────

    [Fact]
    public void RotateCW_RotatesBothPositionAndDirection()
    {
        var tf = new HexTransform(new HexPos(0, 1), HexDirection.Up);
        var rotated = tf.RotateCW();
        rotated.Position.Should().Be(new HexPos(0, 1).RotateCW());   // (1, 0)
        rotated.Direction.Should().Be(HexDirection.UpRight);
    }

    [Fact]
    public void Rotate_SixSteps_IsIdentity()
    {
        var tf = new HexTransform(new HexPos(2, -1), HexDirection.DownRight);
        tf.Rotate(6).Should().Be(tf);
    }

    // ── Rebase ───────────────────────────────────────────────────────────────

    [Fact]
    public void Rebase_IdentityBase_IsUnchanged()
    {
        var tf = new HexTransform(new HexPos(1, 2), HexDirection.UpRight);
        tf.Rebase(HexTransform.Identity).Should().Be(tf);
    }

    [Fact]
    public void Rebase_OffsetBase_ShiftsPosition()
    {
        // Transform at origin facing Up, rebased onto (3,1,Up) → just shifts.
        var tf   = new HexTransform(new HexPos(1, 0), HexDirection.Up);
        var base_ = new HexTransform(new HexPos(3, 1), HexDirection.Up);
        var result = tf.Rebase(base_);
        result.Position.Should().Be(new HexPos(4, 1));
        result.Direction.Should().Be(HexDirection.Up);
    }

    [Fact]
    public void Rebase_RotatedBase_RotatesRelativePosition()
    {
        // One step "forward" in a frame facing UpRight should land on UpRight neighbour.
        var oneStepForward = new HexTransform(new HexPos(0, 1), HexDirection.Up);
        var facingUpRight  = new HexTransform(HexPos.Origin, HexDirection.UpRight);

        var result = oneStepForward.Rebase(facingUpRight);

        // (0,1) rotated 1× CW = (1,0) = the UpRight offset ✓
        result.Position.Should().Be(new HexPos(1, 0));
        result.Direction.Should().Be(HexDirection.UpRight);
    }

    [Fact]
    public void Rebase_ComposesCorrectly()
    {
        // Place a unit 2 steps forward and 1 right relative to a base at (5,5) facing DownRight.
        // "forward" in DownRight frame is the DownRight direction = CW×2 of Up.
        var localPos  = new HexPos(0, 1).Rotate(2) + new HexPos(1, 0).Rotate(2);
        // That's the expected absolute offset; Rebase should give us base.pos + localPos.
        var relative = new HexTransform(new HexPos(0, 1) + new HexPos(1, 0), HexDirection.Up);
        var @base    = new HexTransform(new HexPos(5, 5), HexDirection.DownRight);

        var result = relative.Rebase(@base);
        result.Position.Should().Be(new HexPos(5, 5) + localPos);
    }

    [Fact]
    public void Rebase_IdentityTransform_ReturnsBase()
    {
        // Rebasing Identity onto any base should return that base.
        var @base = new HexTransform(new HexPos(2, -3), HexDirection.DownLeft);
        HexTransform.Identity.Rebase(@base).Should().Be(@base);
    }
}
