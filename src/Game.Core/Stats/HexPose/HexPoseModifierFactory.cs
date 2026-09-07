using Game.Core;
using MatchEngine;

public class HexPoseModifierFactory(GameFlowManager gameFlowManager)
{
    public RotateStaticHexPoseModifier RotateStatic(int clockwiseRotations, bool isAura = true) =>
        new(clockwiseRotations, gameFlowManager.Now, isAura);

    public RotateAroundCenterHexPoseModifier RotateAroundCenter(int clockwiseRotations, bool isAura = true) =>
        new(clockwiseRotations, gameFlowManager.Now, isAura);

    public MoveRelativeHexPoseModifier MoveRelative(HexPosition offset, bool isAura = true) =>
        new(offset, gameFlowManager.Now, isAura);

    public MoveGlobalHexPoseModifier MoveGlobal(HexPosition offset, bool isAura = true) =>
        new(offset, gameFlowManager.Now, isAura);

    public MultiplyVelocityHexPoseModifier MultiplyVelocity(int multiplier, bool isAura = true) =>
        new(multiplier, gameFlowManager.Now, isAura);

    public SetPositionHexPoseModifier SetPosition(HexPosition position, bool isAura = true) =>
        new(position, gameFlowManager.Now, isAura);

    public SetRotationHexPoseModifier SetRotation(HexDirection direction, bool isAura = true) =>
        new(direction, gameFlowManager.Now, isAura);

    public SetPoseHexPoseModifier SetPose(HexPose pose, bool isAura = true) =>
        new(pose, gameFlowManager.Now, isAura);
}
