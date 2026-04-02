namespace Systems.Damage
{
    public interface IDamageService
    {
        public DamageExecutingContext GetSimulatedDamage(DamageTriggeringInfo info);
    }
}