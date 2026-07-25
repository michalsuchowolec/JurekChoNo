using FluentAssertions;

namespace Game.Tests;

public class HexPositionTests
{
    private static readonly HexDirection[] AllDirections =
    {
        HexDirection.Up,
        HexDirection.UpRight,
        HexDirection.DownRight,
        HexDirection.Down,
        HexDirection.DownLeft,
        HexDirection.UpLeft
    };

    // ── Arithmetic ───────────────────────────────────────────────────────────

    [Fact]
    public void Addition_CombinesCoordinates()
    {
        var result = new HexPosition { Q = 1, R = 2 } + new HexPosition { Q = 3, R = -1 };
        result.Should().Be(new HexPosition { Q = 4, R = 1 });
    }

    // ── Direction offsets ────────────────────────────────────────────────────

    [Theory]
    [InlineData(HexDirection.Up,        0,  1)]
    [InlineData(HexDirection.UpRight,   1,  0)]
    [InlineData(HexDirection.DownRight, 1, -1)]
    [InlineData(HexDirection.Down,      0, -1)]
    [InlineData(HexDirection.DownLeft, -1,  0)]
    [InlineData(HexDirection.UpLeft,   -1,  1)]
    public void Direction_UnitVector_MatchesExpected(HexDirection dir, int q, int r)
    {
        HexPosition.UnitVector(dir).Should().Be(new HexPosition { Q = q, R = r });
    }

    // ── Rotation ─────────────────────────────────────────────────────────────

    [Fact]
    public void RotateClockWiseAroundCenter_CyclesThroughDirectionUnitVectors()
    {
        // Each direction's unit vector should rotate CW into the next direction's unit vector.
        for (int i = 0; i < AllDirections.Length; i++)
        {
            var current = HexPosition.UnitVector(AllDirections[i]);
            var nextCW  = HexPosition.UnitVector(AllDirections[(i + 1) % AllDirections.Length]);
            current.RotateClockWiseAroundCenter().Should().Be(nextCW);
        }
    }

    [Fact]
    public void RotateClockWiseAroundCenter_SixSteps_IsIdentity()
    {
        var pos = new HexPosition { Q = 3, R = -1 };
        var rotated = pos;
        for (int i = 0; i < 6; i++)
            rotated = rotated.RotateClockWiseAroundCenter();
        rotated.Should().Be(pos);
    }
}

public class HexPoseTests
{
    // ── Rotate ───────────────────────────────────────────────────────────────

    [Fact]
    public void RotateClockWiseAroundCenter_RotatesBothPositionAndDirection()
    {
        var pose = new HexPose { Position = new HexPosition { Q = 0, R = 1 }, Direction = HexDirection.Up };
        var rotated = pose.RotateClockWiseAroundCenter();
        rotated.Position.Should().Be(new HexPosition { Q = 0, R = 1 }.RotateClockWiseAroundCenter());
        rotated.Direction.Should().Be(HexDirection.UpRight);
    }

    [Fact]
    public void RotateClockWiseAroundCenter_SixSteps_IsIdentity()
    {
        var pose = new HexPose { Position = new HexPosition { Q = 2, R = -1 }, Direction = HexDirection.DownRight };
        pose.RotateClockWiseAroundCenter(6).Should().Be(pose);
    }

    // ── Transform (rebase onto another pose) ────────────────────────────────

    [Fact]
    public void Transform_IdentityOrigin_LeavesPointUnchanged()
    {
        var identity = new HexPose { Position = new HexPosition { Q = 0, R = 0 }, Direction = HexDirection.Up };
        var pose = new HexPose { Position = new HexPosition { Q = 1, R = 2 }, Direction = HexDirection.UpRight };
        (identity * pose).Should().Be(pose);
    }

    [Fact]
    public void Transform_OffsetOrigin_ShiftsPosition()
    {
        // Pose at (1,0) facing Up, transformed onto an origin at (3,1) facing Up → just shifts.
        var pose   = new HexPose { Position = new HexPosition { Q = 1, R = 0 }, Direction = HexDirection.Up };
        var origin = new HexPose { Position = new HexPosition { Q = 3, R = 1 }, Direction = HexDirection.Up };
        var result = origin * pose;
        result.Position.Should().Be(new HexPosition { Q = 4, R = 1 });
        result.Direction.Should().Be(HexDirection.Up);
    }

    [Fact]
    public void Transform_RotatedOrigin_RotatesRelativePosition()
    {
        // One step "forward" transformed into a frame facing UpRight should land on the UpRight neighbour.
        var oneStepForward = new HexPose { Position = new HexPosition { Q = 0, R = 1 }, Direction = HexDirection.Up };
        var facingUpRight  = new HexPose { Position = new HexPosition { Q = 0, R = 0 }, Direction = HexDirection.UpRight };

        var result = facingUpRight * oneStepForward;

        // (0,1) rotated 1x CW = (1,0) = the UpRight unit vector.
        result.Position.Should().Be(new HexPosition { Q = 1, R = 0 });
        result.Direction.Should().Be(HexDirection.UpRight);
    }

    [Fact]
    public void Transform_ComposesCorrectly()
    {
        // Place a pose 2 steps forward + 1 right relative to an origin at (5,5) facing DownRight.
        var localPos = new HexPosition { Q = 0, R = 1 }.RotateClockWiseAroundCenter().RotateClockWiseAroundCenter()
                     + new HexPosition { Q = 1, R = 0 }.RotateClockWiseAroundCenter().RotateClockWiseAroundCenter();
        var relative = new HexPose
        {
            Position = new HexPosition { Q = 0, R = 1 } + new HexPosition { Q = 1, R = 0 },
            Direction = HexDirection.Up
        };
        var origin = new HexPose { Position = new HexPosition { Q = 5, R = 5 }, Direction = HexDirection.DownRight };

        var result = origin * relative;
        result.Position.Should().Be(new HexPosition { Q = 5, R = 5 } + localPos);
    }

    [Fact]
    public void Transform_IdentityPoint_ReturnsOrigin()
    {
        var identity = new HexPose { Position = new HexPosition { Q = 0, R = 0 }, Direction = HexDirection.Up };
        var origin = new HexPose { Position = new HexPosition { Q = 2, R = -3 }, Direction = HexDirection.DownLeft };
        (origin * identity).Should().Be(origin);
    }
}
