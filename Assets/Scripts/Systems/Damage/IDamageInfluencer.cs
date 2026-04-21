using System.Collections.Generic;

namespace Systems.Damage
{
    public interface IDamageInfluencer
    {
        public string DisplayName { get; }
        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context);
    }
}