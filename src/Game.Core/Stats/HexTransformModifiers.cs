using Game.Core;

public abstract class HexTransformModifier : Modifier<HexTransform>{
    public HexTransformModifierType Type;
}

public class HexTransformOffsetModifier : HexTransformModifier
{
    public new HexTransformModifierType Type = HexTransformModifierType.Offset;
    public HexPos RelativeOffset;

    public HexTransformOffsetModifier(TimeStamp timeStamp, bool isAura, HexPos relativeOffset){
        Priority = new ModifierPriority(timeStamp, isAura, (int) Type, ModifierPriority.PackIntPair(relativeOffset.X, relativeOffset.Y));
        RelativeOffset = relativeOffset;
    }
    public override HexTransform Transform(HexTransform initialState)
    {
        return new HexTransform(initialState.Position + RelativeOffset.Rotate((int) initialState.Direction), initialState.Direction);
    }
}


public class HexTransformRotateModifier : HexTransformModifier
{
    public new HexTransformModifierType Type = HexTransformModifierType.Rotate;
    public HexDirection RelativeDirection;

    public HexTransformRotateModifier(TimeStamp timeStamp, bool isAura, HexDirection relativeDirection){
        Priority = new ModifierPriority(timeStamp, isAura, (int) Type, (int)relativeDirection);
        RelativeDirection = relativeDirection;
    }
    public override HexTransform Transform(HexTransform initialState)
    {
        return new HexTransform(initialState.Position, initialState.Direction.Rotate((int) RelativeDirection));
    }
}
public enum HexTransformModifierType{
    Offset,
    Rotate,
}