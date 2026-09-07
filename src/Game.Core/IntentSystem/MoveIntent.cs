namespace MatchEngine;

public class CollectingMovementModifiers(PoseEvaluator evaluator, Unit target) : ModifierRequest<HexPose>
{
    public PoseEvaluator Evaluator { get; } = evaluator;
    public Unit Target { get; } = target;
}

public abstract class MoveIntent : Intent
{
    protected UnitSelector _unitSelector;
    public UnitSelector UnitSelector => _unitSelector;

    protected PoseEvaluator _poseEvaluator;
    public PoseEvaluator PoseEvaluator => _poseEvaluator;

    // Needs both Events (for base Intent) and MovementResolver - rather than collecting the two
    // separately, it just asks the moment for both, same as CollisionBehaviour does.
    protected MomentResolver _momentResolver;
    public MovementResolver MovementResolver => _momentResolver.MovementResolver;

    public MoveIntent(MomentResolver momentResolver, UnitSelector unitSelector, PoseEvaluator poseEvaluator) : base(momentResolver.Events)
    {
        _momentResolver = momentResolver;
        _unitSelector = unitSelector;
        _poseEvaluator = poseEvaluator;
        unitSelector.Source = this;
        poseEvaluator.Source = this;
    }

    public override void Perform()
    {
        foreach (var target in _unitSelector.FindTargets())
        {
            RequestMovement(target);
        }
        PublishEvent();
    }

    protected abstract void RequestMovement(Unit target);
    protected abstract void PublishEvent();
}

public record BasicMoveDone(BasicMoveIntent BasicMoveIntent);
public class BasicMoveIntent : MoveIntent
{
    public BasicMoveIntent(MomentResolver momentResolver, UnitSelector unitSelector, PoseEvaluator poseEvaluator)
        : base(momentResolver, unitSelector, poseEvaluator)
    {
    }

    protected override void RequestMovement(Unit target)
    {
        var offset = _poseEvaluator.EvaluatePose(target);
        MovementResolver.RequestBasicMovement(target, offset.Direction);
    }

    protected override void PublishEvent()
    {
        _events.Publish(new BasicMoveDone(this));
    }
}

public record PushDone(PushIntent PushIntent);
public class PushIntent : MoveIntent
{
    public PushIntent(MomentResolver momentResolver, UnitSelector unitSelector, PoseEvaluator poseEvaluator)
        : base(momentResolver, unitSelector, poseEvaluator)
    {
    }

    protected override void RequestMovement(Unit target)
    {
        var offset = _poseEvaluator.EvaluatePose(target);
        MovementResolver.RequestPush(target, offset.Position);
    }

    protected override void PublishEvent()
    {
        _events.Publish(new PushDone(this));
    }
}

public record TeleportDone(TeleportIntent TeleportIntent);
public class TeleportIntent : MoveIntent
{
    public TeleportIntent(MomentResolver momentResolver, UnitSelector unitSelector, PoseEvaluator poseEvaluator)
        : base(momentResolver, unitSelector, poseEvaluator)
    {
    }

    protected override void RequestMovement(Unit target)
    {
        var destination = _poseEvaluator.EvaluatePose(target);
        MovementResolver.RequestTeleport(target, destination);
    }

    protected override void PublishEvent()
    {
        _events.Publish(new TeleportDone(this));
    }
}

public abstract class PoseEvaluator
{
    private MoveIntent _source = null!;
    public MoveIntent Source
    {
        get => _source;
        set => _source = value;
    }

    private EventBus _events;
    public EventBus Events => _events;

    public PoseEvaluator(EventBus events)
    {
        _events = events;
    }

    protected abstract HexPose GetBasePose(Unit target);

    public HexPose EvaluatePose(Unit target)
    {
        var basePose = GetBasePose(target);
        var request = new CollectingMovementModifiers(this, target);
        _events.Publish(request);
        return Stat<HexPose>.Evaluate(basePose, request.Modifiers);
    }
}

public class FixedPoseEvaluator : PoseEvaluator
{
    private HexPose _pose;
    public HexPose Pose => _pose;

    public FixedPoseEvaluator(EventBus events, HexPose pose) : base(events)
    {
        _pose = pose;
    }

    protected override HexPose GetBasePose(Unit target)
    {
        return _pose;
    }
}
