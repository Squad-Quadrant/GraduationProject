using System.Collections.Generic;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Systems.Damage
{
    public class ShootHitRateInfluence : DamageInfluence
    {
        public Unit.Unit UnitAttacker => Attacker as Unit.Unit;
        private bool _preciseShootSpeed;
        public ShootHitRateInfluence(IDamageInfluencer owner, int priority = 0, bool preciseShootSpeed = false) : base(owner, priority)
        {
            _preciseShootSpeed = preciseShootSpeed;
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.HitRate };

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
            var theWeapon = ((WeaponLogic)Owner);
            
            // 获得两个单位的距离，曼哈顿距离
            float distance = Mathf.Abs(UnitAttacker.position.x - Defender.position.x) + Mathf.Abs(UnitAttacker.position.y - Defender.position.y);
            
            float hitRateMultiplier = 1f;
            var hitRange = theWeapon.ShotRange();
            foreach (var theRange in hitRange)
            {
                if (distance >= theRange.min)
                {
                    hitRateMultiplier = theRange.hitRate;
                }
                else
                {
                    break;
                }
            }
            
            // Context.HitRate *= hitRateMultiplier;
            Context.AddHitRateInfluence("射程修正", 0, hitRateMultiplier);

            if (_preciseShootSpeed)
            {
                Context.AddHitRateInfluence("精准射击", theWeapon.PreciseShootHitRateBonus());
                // Context.HitRate += theWeapon.PreciseShootHitRateBonus();
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