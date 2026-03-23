using Systems.Equipment;
using UnityEngine;

namespace Systems.Damage
{
    public class ShotHitRateInfluence : DamageInfluence
    {
        public ShotHitRateInfluence(IDamageInfluencer owner, int priority = 0) : base(owner, priority)
        {
        }

        public override void Execute()
        {
            var theWeapon = ((WeaponLogic)Owner);
            
            // 获得两个单位的距离
            float distance = Vector2Int.Distance(Attacker.position, Defender.position);
            float hitRateMultiplier = 1f;
            var hitRange = theWeapon.ShotRange();
            foreach (var theRange in hitRange)
            {
                if (distance >= theRange.min)
                {
                    hitRateMultiplier = theRange.hitRate;
                    break;
                }
            }
            
            Context.HitRate *= hitRateMultiplier;
        }
    }
}