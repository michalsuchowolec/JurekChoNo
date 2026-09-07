using FluentAssertions;
using Game.Core;
using MatchEngine;

namespace Game.Tests;

public class MovementResolverTests
{
    [Fact]
    public void RequestBasicMovement_MovesUnitOneStepInDirection()
    {
        // NOTE: MatchRoot is only here to get a MomentResolver going - none of MatchRoot's
        // players/decks/phases are exercised by this test at all.
        var matchRoot = new MatchRoot(new List<InitialPlayerData> { new(new List<string>()), new(new List<string>()) });
        var unit = new Unit(matchRoot.IdManager.NextEntityId(), matchRoot.Events);
        matchRoot.MatchState.Board.Place(unit, HexPose.Identity);

        // Drain() runs ProcessMovement() to completion on its own, so there's no separate loop to
        // drive here - queuing the request through the seed callback is enough.
        MomentResolver.ResolveMoment(matchRoot, resolver =>
            resolver.MovementResolver.RequestBasicMovement(unit, HexDirection.Up));

        unit.Pose.Position.Should().Be(HexPosition.UnitVector(HexDirection.Up));
    }
}
