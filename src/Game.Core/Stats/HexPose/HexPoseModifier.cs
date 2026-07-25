using Game.Core;

public abstract class HexPoseModifier : Modifier<HexPose>
{
    public HexPoseModifierType Type { get; }

    protected HexPoseModifier(HexPoseModifierType type, TimeStamp timeStamp, bool isAura)
    {
        Type = type;
        Priority = new ModifierPriority(timeStamp, isAura, (int)type);
    }
}

public enum HexPoseModifierType
{
    RotateStatic,
    RotateAroundCenter,
    MoveRelative,
    MoveGlobal,
    MultiplyVelocity,
    SetRotation,
    SetPosition,
    SetPose
}

/*
Ok, so basically there needs to be a couple of versions of modifiers:
    - Rotate unit. Happens first. (rotation disruption)
    - Rotate Around Center
    - relative move happens second. (move)
    - global move happens last (push)
    - Multpiply Velocity (not happening with others, used on meta pose, not pose of unit, but might happen on pose of intent)
    - Set Position
    - Set Rotation
    - Set Pose


Note: Transform can't be an operation, because M1 * M2 * V1 != M2 * M1 * V1, it breaks Commutative Proerty! It needs to be split between offset and rotation!


Now meta modifiers that operate on intent:
    - double distance / root
*/

// REEVALUATE: RotateStatic is currently one type/priority tier used for any static rotation, but the
// pipeline above needs *two* distinct rotation stages (initial rotation, and a second rotation that
// applies after the move) that can be disrupted independently by different effects. ModifierPriority.CompareTo
// only reaches OperationPriority (== (int)Type here) as a tiebreaker below IsAura/TimeStamp, so two
// RotateStatic modifiers meant for different stages currently have no way to sort relative to each other
// on purpose. Needs either separate modifier types per rotation stage or a rethought priority scheme -
// see the matching note on MovementResolver.RequestRelativeMovement.
