using System.Collections.Generic;

namespace Systems.Damage
{
    public interface IDamageInfluencer
    {
        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context);
    }
}