using Game.Core;

public interface IDamagable
{
    public void TakeDamage(int amount);
    public EntityId Id { get; set; }
}
