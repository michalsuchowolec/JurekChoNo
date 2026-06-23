namespace Game.Core;

/// <summary>
/// The six directions, ordered clockwise from Up.
/// The enum value equals the number of 60° CW steps from Up,
/// so rotation arithmetic is just modular integer addition.
/// </summary>
public enum HexDirection
{
    Up        = 0,
    UpRight   = 1,
    DownRight = 2,
    Down      = 3,
    DownLeft  = 4,
    UpLeft    = 5,
}

/// <summary>
/// Axial hex position using two oblique axes:
///   X axis → UpRight  (unit vector (1, 0))
///   Y axis → Up       (unit vector (0, 1))
///
/// All six neighbours are reachable with one X or Y step:
///   Up=(0,1)  UpRight=(1,0)  DownRight=(1,-1)
///   Down=(0,-1)  DownLeft=(-1,0)  UpLeft=(-1,1)
/// </summary>
public readonly record struct HexPos(int X, int Y)
{
    public static readonly HexPos Origin = new(0, 0);

    public static HexPos operator +(HexPos a, HexPos b) => new(a.X + b.X, a.Y + b.Y);
    public static HexPos operator -(HexPos a, HexPos b) => new(a.X - b.X, a.Y - b.Y);
    public static HexPos operator -(HexPos a)           => new(-a.X, -a.Y);

    /// <summary>Rotate 60° clockwise around the origin: (x,y) → (x+y, -x)</summary>
    public HexPos RotateCW() => new(X + Y, -X);

    /// <summary>Rotate 60° counter-clockwise around the origin: (x,y) → (-y, x+y)</summary>
    public HexPos RotateCCW() => new(-Y, X + Y);

    /// <summary>Rotate by <paramref name="steps"/> × 60° CW (negative = CCW).</summary>
    public HexPos Rotate(int steps)
    {
        steps = ((steps % 6) + 6) % 6;
        var p = this;
        for (int i = 0; i < steps; i++) p = p.RotateCW();
        return p;
    }

    public static int Distance(HexPos a, HexPos b)
    {
        int dx = b.X - a.X, dy = b.Y - a.Y;
        return (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dx + dy)) / 2;
    }

    public int DistanceTo(HexPos other) => Distance(this, other);

    public HexPos Neighbor(HexDirection dir) => this + dir.ToOffset();

    public IEnumerable<HexPos> Neighbors()
    {
        foreach (HexDirection dir in HexDirectionExt.All)
            yield return Neighbor(dir);
    }

    public override string ToString() => $"({X},{Y})";
}

public static class HexDirectionExt
{
    public static readonly HexDirection[] All =
    {
        HexDirection.Up,
        HexDirection.UpRight,
        HexDirection.DownRight,
        HexDirection.Down,
        HexDirection.DownLeft,
        HexDirection.UpLeft
    };

    private static readonly HexPos[] Offsets =
    {
        new( 0,  1),  // Up
        new( 1,  0),  // UpRight
        new( 1, -1),  // DownRight
        new( 0, -1),  // Down
        new(-1,  0),  // DownLeft
        new(-1,  1)  // UpLeft
    };

    public static HexPos ToOffset(this HexDirection dir) => Offsets[(int)dir];

    public static HexDirection Opposite(this HexDirection dir) =>
        (HexDirection)(((int)dir + 3) % 6);

    /// <summary>Rotate by <paramref name="steps"/> × 60° CW (negative = CCW).</summary>
    public static HexDirection Rotate(this HexDirection dir, int steps) =>
        (HexDirection)(((int)dir + steps % 6 + 6) % 6);
}

/// <summary>
/// A position combined with a facing direction — a local coordinate frame on the hex grid.
/// </summary>
public readonly record struct HexTransform(HexPos Position, HexDirection Direction)
{
    public static readonly HexTransform Identity = new(HexPos.Origin, HexDirection.Up);

    /// <summary>Rotate the entire transform (position and facing) 60° CW around the origin.</summary>
    public HexTransform RotateCW()  => new(Position.RotateCW(),  Direction.Rotate(+1));

    /// <summary>Rotate the entire transform (position and facing) 60° CCW around the origin.</summary>
    public HexTransform RotateCCW() => new(Position.RotateCCW(), Direction.Rotate(-1));

    /// <summary>Rotate by <paramref name="steps"/> × 60° CW (negative = CCW).</summary>
    public HexTransform Rotate(int steps) =>
        new(Position.Rotate(steps), Direction.Rotate(steps));

    /// <summary>
    /// Re-express this transform — currently relative to (Origin, Up) — so that it has
    /// the same relative position and facing with respect to <paramref name="base"/>.
    ///
    /// Concretely: rotate this transform's position and direction by the base's orientation,
    /// then offset by the base's position.
    /// </summary>
    public HexTransform Rebase(HexTransform @base)
    {
        int steps = (int)@base.Direction;
        return new(@base.Position + Position.Rotate(steps), Direction.Rotate(steps));
    }
}
