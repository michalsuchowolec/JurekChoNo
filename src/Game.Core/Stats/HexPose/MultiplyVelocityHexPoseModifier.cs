using Game.Core;

public class MultiplyVelocityHexPoseModifier : HexPoseModifier
{
    private int _multiplier;

    public MultiplyVelocityHexPoseModifier(int multiplier, TimeStamp timeStamp, bool isAura)
        : base(HexPoseModifierType.MultiplyVelocity, timeStamp, isAura)
    {
        _multiplier = multiplier;
    }

    public override HexPose Transform(HexPose initialState)
    {
        return new HexPose
        {
            Position = new HexPosition { Q = initialState.Position.Q * _multiplier, R = initialState.Position.R * _multiplier },
            Direction = initialState.Direction
        };
    }
}
