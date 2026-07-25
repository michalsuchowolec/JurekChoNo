namespace MatchEngine;
public abstract class UnitSelector
{
    private Intent _source = null!;
    public Intent Source
    {
        get => _source;
        set => _source = value;
    }

    public abstract List<Unit> FindTargets();
}
