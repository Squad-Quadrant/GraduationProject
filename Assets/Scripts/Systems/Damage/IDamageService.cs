using System.Collections.Generic;

namespace Systems.Damage
{
    public interface IDamageService
    {
        public DamageExecutingContext GetSimulatedDamage(BulletDamageTriggeringInfo info, out Dictionary<BodyPartType, DamageExecutingContext> simulatedBodyPartDamages);
    }
}