using Systems.Damage;
using UnityEngine;

namespace Systems.Buff.Config
{
    [CreateAssetMenu(fileName = "RemoveHitRateInfluence", menuName = "Game/Buff/BuffEvent/RemoveHitRateInfluence")]
    public class RemoveAllHitRateInfluence : UnitBuffEvent
    {
        protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
        {
			unit.DamageInfluences.RemoveAll(influence => (influence as BuffHitRateInfluence).RealOwner == buffInfo);
        }
    }
}