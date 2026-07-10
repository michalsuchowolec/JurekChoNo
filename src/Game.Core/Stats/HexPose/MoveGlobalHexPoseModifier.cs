using Game.Core;

public class MoveGlobalHexPoseModifier : HexPoseModifier
{
    private HexPosition _offset;

    public MoveGlobalHexPoseModifier(HexPosition offset, TimeStamp timeStamp, bool isAura)
        : base(HexPoseModifierType.MoveGlobal, timeStamp, isAura)
    {
        _offset = offset;
    }

    public override HexPose Transform(HexPose initialState)
    {
        return initialState + _offset;
    }
}
