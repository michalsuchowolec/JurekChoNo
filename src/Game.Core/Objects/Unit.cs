using Game.Core;


public class Unit{
    public Stat<Health> Health;
    public Stat<int> Speed;
    public Stat<int> Attack;
    public Stat<HexTransform> Transform;
    public List<Effect> Effects = new();
    public void AddEffect(Effect effect){
        Effects.Add(effect);
    }
}
