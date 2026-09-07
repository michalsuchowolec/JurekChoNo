public interface IEffectsOwner
{
    public IEnumerable<Effect> Effects{get;}
    public void AddEffect(Effect effect);
}