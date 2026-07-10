using Game.Core;
using MatchEngine;

public class HexPoseModifierFactory(TimeStampTracker timeStampTracker)
{
    public RotateStaticHexPoseModifier RotateStatic(int clockwiseRotations, bool isAura = true) =>
        new(clockwiseRotations, timeStampTracker.Now, isAura);

    public RotateAroundCenterHexPoseModifier RotateAroundCenter(int clockwiseRotations, bool isAura = true) =>
        new(clockwiseRotations, timeStampTracker.Now, isAura);

    public MoveRelativeHexPoseModifier MoveRelative(HexPosition offset, bool isAura = true) =>
        new(offset, timeStampTracker.Now, isAura);

    public MoveGlobalHexPoseModifier MoveGlobal(HexPosition offset, bool isAura = true) =>
        new(offset, timeStampTracker.Now, isAura);

    public MultiplyVelocityHexPoseModifier MultiplyVelocity(int multiplier, bool isAura = true) =>
        new(multiplier, timeStampTracker.Now, isAura);

    public SetPositionHexPoseModifier SetPosition(HexPosition position, bool isAura = true) =>
        new(position, timeStampTracker.Now, isAura);

    public SetRotationHexPoseModifier SetRotation(HexDirection direction, bool isAura = true) =>
        new(direction, timeStampTracker.Now, isAura);

    public SetPoseHexPoseModifier SetPose(HexPose pose, bool isAura = true) =>
        new(pose, timeStampTracker.Now, isAura);
}
