using Systems.Damage;
using UnityEngine;

namespace Systems.Buff.Config
{
    [CreateAssetMenu(fileName = "AddHitRateInfluence", menuName = "Game/Buff/BuffEvent/AddHitRateInfluence")]
    public class HitRateInfluence : UnitBuffEvent
    {
        public float changer = 0;
        public float multiplier = 1;
        protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
        {
            unit.DamageInfluences.Add(new BuffHitRateInfluence(multiplier, changer, buffInfo, unit));
        }
    }
}