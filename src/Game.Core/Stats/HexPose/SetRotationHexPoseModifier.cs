using Game.Core;

public class SetRotationHexPoseModifier : HexPoseModifier
{
    private readonly HexDirection _direction;

    public SetRotationHexPoseModifier(HexDirection direction, TimeStamp timeStamp, bool isAura)
        : base(HexPoseModifierType.SetRotation, timeStamp, isAura)
    {
        _direction = direction;
    }

    public override HexPose Transform(HexPose initialState)
    {
        return new HexPose
        {
            Position = initialState.Position,
            Direction = _direction
        };
    }
}
