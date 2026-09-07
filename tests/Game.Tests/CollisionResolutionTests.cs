using FluentAssertions;
using Game.Core;
using MatchEngine;

namespace Game.Tests;

public class CollisionResolutionTests
{
    // This is the scope MovementResolverTests.cs can't reach: MovementResolver deliberately
    // hands control back to MomentResolver's Drain loop when it queues a collision (so damage
    // intents, effects, etc. can resolve before positions are finalized). So "did colliding
    // units survive and end up back where they started" is a property of the whole moment
    // pipeline, not of MovementResolver alone - it's driven through MomentResolver.ResolveMoment,
    // but still bypasses GameFlowManager's turn/phase machinery entirely (no ready-checks, no
    // phase advancing) by handing hand-built decisions straight to the resolver.
    [Fact(Timeout = 5000)]
    public void UnitsConvergingOnSameTile_WithNoCollisionBehaviours_BothReturnToOriginalPose()
    {
        var matchRoot = new MatchRoot(new List<InitialPlayerData> { new(new List<string>()), new(new List<string>()) });
        var playerA = matchRoot.MatchState.Players[0];
        var playerC = matchRoot.MatchState.Players[1];

        var unitA = new Unit(matchRoot.IdManager.NextEntityId(), matchRoot.Events) { Owner = playerA };
        var unitC = new Unit(matchRoot.IdManager.NextEntityId(), matchRoot.Events) { Owner = playerC };

        // Both units face Up and both target (0,0), which is empty - neither is "in the other's
        // way" at the start, they just both try to step into the same tile.
        var poseA = new HexPose { Position = new HexPosition { Q = -1, R = 1 }, Direction = HexDirection.Up };
        var poseC = new HexPose { Position = new HexPosition { Q = 1, R = 0 }, Direction = HexDirection.Up };
        matchRoot.MatchState.Board.Place(unitA, poseA);
        matchRoot.MatchState.Board.Place(unitC, poseC);

        MomentResolver.ResolveMoment(matchRoot, resolver =>
        {
            resolver.Intents.QueueIntent(new BasicMoveIntent(
                resolver,
                new SingleUnitSelector(unitA),
                new FixedPoseEvaluator(resolver.Events, new HexPose { Direction = HexDirection.DownRight })));
            resolver.Intents.QueueIntent(new BasicMoveIntent(
                resolver,
                new SingleUnitSelector(unitC),
                new FixedPoseEvaluator(resolver.Events, new HexPose { Direction = HexDirection.DownLeft })));
        });

        unitA.Pose.Should().Be(poseA);
        unitC.Pose.Should().Be(poseC);
    }
}
