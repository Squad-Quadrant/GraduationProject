using System.Collections.Generic;
using Systems.Buff;

namespace Systems.Damage
{
    public class BuffHitRateInfluence : DamageInfluence
    {
        private float _hitRateMultiplier;
        private float _hitRateChanger;
        private BuffInfo _realOwner; // 用于未来识别哪个Buff的影响
        public BuffInfo RealOwner => _realOwner;
        public BuffHitRateInfluence(float multiplier, float changer, BuffInfo realOwner, IDamageInfluencer owner, int priority = 0) : base(owner, priority)
        {
            _hitRateMultiplier = multiplier;
            _realOwner = realOwner;
            _hitRateChanger = changer;
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.HitRate, DamageInfluenceType.Buff };

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
            Context.HitRate += _hitRateChanger;
            Context.HitRate *= _hitRateMultiplier;
        }

        public override void Execute()
        {

        }

        public override void Last()
        {
            
        }
    }
}