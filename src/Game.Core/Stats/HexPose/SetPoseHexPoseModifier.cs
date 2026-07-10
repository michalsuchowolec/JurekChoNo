using Game.Core;

public class SetPoseHexPoseModifier : HexPoseModifier
{
    private HexPose _pose;

    public SetPoseHexPoseModifier(HexPose pose, TimeStamp timeStamp, bool isAura)
        : base(HexPoseModifierType.SetPose, timeStamp, isAura)
    {
        _pose = pose;
    }

    public override HexPose Transform(HexPose initialState)
    {
        return _pose;
    }
}
