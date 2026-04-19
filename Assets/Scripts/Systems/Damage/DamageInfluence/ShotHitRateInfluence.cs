using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Systems.Damage
{
    public class ShotHitRateInfluence : DamageInfluence
    {
        private bool _preciseShootSpeed;
        public ShotHitRateInfluence(IDamageInfluencer owner, int priority = 0, bool preciseShootSpeed = false) : base(owner, priority)
        {
            _preciseShootSpeed = preciseShootSpeed;
        }

        public override DamageInfluenceType DamageInfluenceType => DamageInfluenceType.HitRate;

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
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

            if (_preciseShootSpeed)
            {
                Context.HitRate += theWeapon.PreciseShootHitRateBonus();
            }
        }

        public override void Execute()
        {

        }

        public override void Last()
        {
            
        }
    }
}