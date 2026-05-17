using System.Collections.Generic;

namespace Systems.Damage
{
    public class RecoverInfluence : DamageInfluence
    {
        // private float _damageMultiplier;
        private int _damageChanger;
        public RecoverInfluence(int changer, IDamageInfluencer owner, int priority = 0) : base(owner, priority)
        {
            // _damageMultiplier = multiplier;
            _damageChanger = changer;
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.HitRate };

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
        }

        public override void Execute()
        {
            Context.Damage += _damageChanger;
            // Context.Damage += Mathf.RoundToInt(Context.Defender.CurrentHp * (1 - _damageMultiplier));
        }

        public override void Last()
        {
            
        }
    }
}