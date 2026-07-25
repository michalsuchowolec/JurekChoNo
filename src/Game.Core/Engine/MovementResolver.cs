namespace MatchEngine;
public class MovementResolver{

    private Board _board;
    private HexPoseModifierFactory _hexPoseModifierFactory;
    private Dictionary<Unit, List<Modifier<HexPose>>> _plannedMoves = new();
    private Dictionary<Unit, List<Unit>> _collisions = new();
    public MovementResolver(Board board, HexPoseModifierFactory hexPoseModifierFactory)
    {
        _board = board;
        _hexPoseModifierFactory = hexPoseModifierFactory;
    }

    public bool ProcessMovement(){
        if(RealizePosesAfterCollisions()) return true;
        if(QueueCombat()) return true;
        if(MoveRemainingPieces()) return true;
        return false;
    }

    private HexPose FindDestinationPose(Unit unit)
    {
        if (!_plannedMoves.ContainsKey(unit))
        {
            return unit.Pose;
        }
        var move = Stat<HexPose>.Evaluate(HexPose.Identity, _plannedMoves[unit]);
        return unit.Pose * move;
    }
    private bool RealizePosesAfterCollisions()
    {
        if(_collisions == null || _collisions.Count == 0) return false;
        foreach(var unit in _collisions.Keys)
        {
            var destinationPose = FindDestinationPose(unit);
            if(_collisions[unit].Count == 0 && _board.IsInBounds(destinationPose * unit.CollisionArea))
            {
                _board.Move(unit, unit.Pose, FindDestinationPose(unit));    
            }
            _plannedMoves.Remove(unit);
        }
        return true;
    }

    private bool MoveRemainingPieces()
    {
        if(_plannedMoves.Count == 0) return false;
        foreach(var unit in _plannedMoves.Keys)
        {
            _board.Move(unit, unit.Pose, FindDestinationPose(unit));
            _plannedMoves.Remove(unit);
        }
        return true;
    }

    private bool QueueCombat()
    {
        FindCollisions();
        if(_collisions.Count == 0) return false;
        foreach(var collision in _collisions)
        {
            foreach(var opponent in collision.Value)
            {
                collision.Key.Collide(opponent);
            }
        }
        return true;
    }

    private void FindCollisions()
    {
        _collisions.Clear();
        var units = _board.Units;
        for(int i = 0; i < units.Count - 1; i++)
        {
            for(int j = i + 1; j < units.Count; j++)
            {
                var a = units[i];
                var b = units[j];
                if(DoUnitsCrossPaths(a, b) || DoUnitsOverlapAfterMovement(a, b))
                {
                    if(!_collisions.ContainsKey(a)) _collisions[a] = new();
                    if(!_collisions.ContainsKey(b)) _collisions[b] = new();
                    _collisions[a].Add(b);
                    _collisions[b].Add(a);
                }
            }
        }
    }

    private bool DoUnitsCrossPaths(Unit a, Unit b)
    {
        if(!_plannedMoves.ContainsKey(a) || !_plannedMoves.ContainsKey(b)) return false; //both units need to move
        
        var aMovement = Stat<HexPose>.Evaluate(HexPose.Identity, _plannedMoves[a]);
        var bMovement = Stat<HexPose>.Evaluate(HexPose.Identity, _plannedMoves[b]);
        if(aMovement.Magnitude == 0 || bMovement.Magnitude == 0) return false; //movement must include offset, not only rotation.     
        var aInitialArea = a.Pose * a.CollisionArea;
        var bInitialArea = b.Pose * b.CollisionArea;
        if(aInitialArea.Distance(bInitialArea) > 1) return false; //cross collisions don't happen between long range moves
        var aDestinationOrigin = a.Pose * aMovement;
        var aDestinationArea = aDestinationOrigin * a.CollisionArea;
        var bDestinationOrigin = b.Pose * bMovement;
        var bDestinationArea = bDestinationOrigin * b.CollisionArea;
        return aInitialArea.Overlaps(bDestinationArea) && bInitialArea.Overlaps(aDestinationArea);
    }

    private bool DoUnitsOverlapAfterMovement(Unit a, Unit b)
    {
        var aMovement = Stat<HexPose>.Evaluate(HexPose.Identity, _plannedMoves[a]);
        var bMovement = Stat<HexPose>.Evaluate(HexPose.Identity, _plannedMoves[b]);
        var aDestinationOrigin = a.Pose * aMovement;
        var aDestinationArea = aDestinationOrigin * a.CollisionArea;
        var bDestinationOrigin = b.Pose * bMovement;
        var bDestinationArea = bDestinationOrigin * b.CollisionArea;
        return aDestinationArea.Overlaps(bDestinationArea);
    }
    private void MoveUnit(Unit unit)
    {   
        _board.Move(
            unit,
            unit.Pose.Position,
            unit.Pose * Stat<HexPose>.Evaluate(HexPose.Identity, _plannedMoves[unit])  //I'm not sure which part is transformation and which position
        );
    }

    // Rotation and local-offset are queued as two independent, order-agnostic modifiers
    // (RotateStatic only ever changes Direction, MoveGlobal only ever changes Position), rather than
    // one RotateStatic + one MoveRelative. MoveRelative reads whatever Direction has already been
    // applied *at the moment it runs*, so its result depends on modifier order - fine when a single
    // request queues both, but not once another intent's modifiers can land on the same unit in the
    // same moment and get sorted into a different order. Resolving the local offset into a global one
    // up front, using the direction this request itself will end up facing, makes the two contributions
    // commutative: applying them in either order, or interleaved with another intent's own
    // rotation/offset pair, always produces the same final pose.
    //
    // REEVALUATE: this only models 2 stages (rotate, then offset). The intended shape is 5 -
    // initial rotation => relative offset => second rotation => global offset => setters - so that
    // effects can disrupt the initial facing, increase distance traveled, or rotate the unit again
    // *after* it moves, independently of each other, and so a unit can move and rotate in the same
    // action (there's already a tabletop keyword for that on the physical prototype). Needs pen-and-paper
    // testing before implementing - see the matching note in HexPoseModifier.cs.
    public void RequestRelativeMovement(Unit unit, int clockwiseRotations, HexPosition localOffset)
    {
        if (!_plannedMoves.ContainsKey(unit))
        {
            _plannedMoves[unit] = new();
        }
        var newDirection = (HexDirection)(((int) unit.Pose.Direction + clockwiseRotations) % 6);
        var globalOffset = HexPosition.Transform(new HexPose { Position = HexPosition.Identity, Direction = newDirection }, localOffset);
        _plannedMoves[unit].Add(_hexPoseModifierFactory.RotateStatic(clockwiseRotations));
        _plannedMoves[unit].Add(_hexPoseModifierFactory.MoveGlobal(globalOffset));
    }

    public void RequestBasicMovement(Unit unit, HexDirection direction)
    {
        RequestRelativeMovement(unit, (int) direction, HexPosition.UnitVector(HexDirection.Up));
    }

    public void RequestPush(Unit unit, HexPosition offset)
    {
        throw new NotImplementedException();
    }

    public void RequestTeleport(Unit unit, HexPose pose)
    {
        throw new NotImplementedException();
    }
}
