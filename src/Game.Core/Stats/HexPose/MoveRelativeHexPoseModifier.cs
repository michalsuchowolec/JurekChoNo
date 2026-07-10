using Game.Core;

public class MoveRelativeHexPoseModifier : HexPoseModifier
{
    private HexPosition _offset;

    public MoveRelativeHexPoseModifier(HexPosition offset, TimeStamp timeStamp, bool isAura)
        : base(HexPoseModifierType.MoveRelative, timeStamp, isAura)
    {
        _offset = offset;
    }

    public override HexPose Transform(HexPose initialState)
    {
        return new HexPose
        {
            Position = initialState * _offset,
            Direction = initialState.Direction
        };
    }
}
