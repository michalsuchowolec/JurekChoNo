public class HealthStat : Stat<Health>
{
    public HealthStat(int maxHealth) : base(new Health(maxHealth)) { }
    public HealthStat(Health baseValue) : base(baseValue) { }
}
