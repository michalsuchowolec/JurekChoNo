using Game.Core;

public class RotateStaticHexPoseModifier : HexPoseModifier
{
    private int _clockwiseRotations;

    public RotateStaticHexPoseModifier(int clockwiseRotations, TimeStamp timeStamp, bool isAura)
        : base(HexPoseModifierType.RotateStatic, timeStamp, isAura)
    {
        _clockwiseRotations = clockwiseRotations;
    }

    public override HexPose Transform(HexPose initialState)
    {
        return initialState.RotateStaticClockwise(_clockwiseRotations);
    }
}
