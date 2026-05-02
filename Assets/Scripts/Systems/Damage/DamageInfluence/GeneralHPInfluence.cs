using System.Collections.Generic;
using UnityEngine;

namespace Systems.Damage
{
    public class GeneralHPInfluence : DamageInfluence
    {
        private float _damageMultiplier;
        private int _damageChanger;
        public GeneralHPInfluence(float multiplier, int changer, IDamageInfluencer owner, int priority = 0) : base(owner, priority)
        {
            _damageMultiplier = multiplier;
            _damageChanger = changer;
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.HitRate };

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
            Context.Damage += _damageChanger;
            Context.Damage += Mathf.RoundToInt(Context.Defender.CurrentHp * (1 - _damageMultiplier));
        }

        public override void Execute()
        {
            
        }

        public override void Last()
        {
            
        }
    }
}