using Game.Core;

public class SetPositionHexPoseModifier : HexPoseModifier
{
    private HexPosition _position;

    public SetPositionHexPoseModifier(HexPosition position, TimeStamp timeStamp, bool isAura)
        : base(HexPoseModifierType.SetPosition, timeStamp, isAura)
    {
        _position = position;
    }

    public override HexPose Transform(HexPose initialState)
    {
        return new HexPose
        {
            Position = _position,
            Direction = initialState.Direction
        };
    }
}
