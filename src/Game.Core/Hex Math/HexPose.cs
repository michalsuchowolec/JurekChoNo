public struct HexPose
{
    public HexPosition Position;
    public HexDirection Direction;
    public static HexPose Identity => new HexPose{Position = HexPosition.Identity, Direction = HexDirection.Up};
    public int Magnitude => Position.Magnitude;

    public HexPose RotateClockWiseAroundCenter()
    {
        return new HexPose
        {
            Position = Position.RotateClockWiseAroundCenter(),
            Direction = (HexDirection) (((int) Direction  + 1) % 6)
        };
    }

    public HexPose RotateClockWiseAroundCenter(int rotations)
    {
        var result = this;
        for(int i = 0; i < rotations; i++)
        {
            result = result.RotateClockWiseAroundCenter();
        }
        return result;
    }

    public HexPose RotateStaticClockwise(int rotations)
    {
        return new HexPose
        {
            Position = Position,
            Direction = (HexDirection)(((int) Direction + rotations) % 6)
        };
    }

    public HexPose Offset(HexPosition offset)
    {
        return new HexPose
        {
            Position = Position.Offset(offset),
            Direction = Direction  
        };
    }

    public static HexPose Transform(HexPose origin, HexPose point)
    {
        var result = point;
        for(int i = 0; i < (int) origin.Direction; i++)
        {
            result = result.RotateClockWiseAroundCenter();
        }
        return result.Offset(origin.Position);
    }


    public static  HexPose operator + (HexPose a, HexPose b)
    {
        return new HexPose
        {
            Position = a.Position + b.Position,
            Direction = (HexDirection)(((int) a.Direction + (int) b.Direction) % 6)
        };
    }

    public static   HexPose operator + (HexPose a, HexPosition b)
    {
        return new HexPose
        {
            Position = a.Position + b,
            Direction = a.Direction
        };
    }

    public static HexPose operator * (HexPose origin, HexPose point)
    {
        return Transform(origin, point);
        
    }

    public override string ToString()
    {
        return $"({Position.Q}, {Position.R}, {Direction})";
    }


}

