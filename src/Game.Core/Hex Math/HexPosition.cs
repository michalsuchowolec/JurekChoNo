using Game.Core;

public struct HexPosition
{
    public static HexPosition Identity => new HexPosition{Q = 0, R = 0};
    public static HexPosition Up => new HexPosition{Q = 0, R = 1};
    public static HexPosition UpRight => new HexPosition{Q = 1, R = 0};
    public static HexPosition DownRight = new HexPosition{Q = 1, R = -1};
    public static HexPosition Down = new HexPosition{Q = 0, R = -1};
    public static HexPosition DownLeft = new(){Q = -1, R = 0};
    public static HexPosition UpLeft = new(){Q = -1, R = 1};

    public int Q;
    public int R;

    public int Magnitude => new int[]
    {
        Math.Abs(Q) + Math.Abs(R), //QR combination
        Math.Abs(Q + R) + Math.Abs(R), //QS combination
        Math.Abs(Q + R) + Math.Abs(Q) //RS combination
    }.Min();
    public HexPosition RotateClockWiseAroundCenter()
    {
        return new HexPosition
        {
            Q = Q + R,
            R = - Q,  
        };
    }

    public HexPosition Offset(HexPosition offset)
    {
        return new HexPosition
            {
                Q = Q + offset.Q,
                R = R + offset.R
        };
    }

    public static HexPosition UnitVector(HexDirection direction) => direction switch
    {
        HexDirection.Up        => Up,
        HexDirection.UpRight   => UpRight,
        HexDirection.DownRight => DownRight,
        HexDirection.Down      => Down,
        HexDirection.DownLeft  => DownLeft,
        HexDirection.UpLeft    => UpLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };

    public static HexPosition Transform(HexPose origin, HexPosition point)
    {
        var result = point;
        for(int i = 0; i < (int) origin.Direction; i++)
        {
            result = result.RotateClockWiseAroundCenter();
        }
        return result.Offset(origin.Position);
    }


    public static HexPosition operator +(HexPosition a, HexPosition b)
    {
        return new HexPosition
        {
            Q = a.Q + b.Q,
            R = a.R + b.R  
        };
    }

    public static HexPosition operator -(HexPosition a, HexPosition b)
    {
        return new HexPosition
        {
            Q = a.Q - b.Q, 
            R = a.R - b.R
        };
    }

    public static HexPosition operator * (HexPose origin, HexPosition point)
    {
        return Transform(origin, point);
    }

    public override string ToString()
    {
        return $"({Q}, {R})";
    }

}