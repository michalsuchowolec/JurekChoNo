using Game.Core;

public class RotateAroundCenterHexPoseModifier : HexPoseModifier
{
    private int _clockwiseRotations;

    public RotateAroundCenterHexPoseModifier(int clockwiseRotations, TimeStamp timeStamp, bool isAura)
        : base(HexPoseModifierType.RotateAroundCenter, timeStamp, isAura)
    {
        _clockwiseRotations = clockwiseRotations;
    }

    public override HexPose Transform(HexPose initialState)
    {
        return initialState.RotateClockWiseAroundCenter(_clockwiseRotations);
    }
}
