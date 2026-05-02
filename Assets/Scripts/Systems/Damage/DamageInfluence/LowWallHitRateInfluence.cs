using System.Collections.Generic;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Systems.Damage
{
    public class LowWallRateInfluence : DamageInfluence
    {
        public Unit.Unit UnitAttacker => Attacker as Unit.Unit;
        public LowWallRateInfluence(IDamageInfluencer owner, int priority = 0) : base(owner, priority)
        {
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.HitRate };

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
            
            Context.AddHitRateInfluence("遮挡掩体", -0.25f);
        }

        public override void Execute()
        {

        }

        public override void Last()
        {
            
        }
    }
}