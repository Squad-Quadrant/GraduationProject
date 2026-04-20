using Systems.Buff;

namespace Systems.Damage
{
    public class BuffHitRateInfluence : DamageInfluence
    {
        private float _hitRateMultiplier;
        private float _hitRateChanger;
        private BuffInfo _realOwner;
        public BuffInfo RealOwner => _realOwner;
        // todo: owner不一定是IDamageInfluencer
        public BuffHitRateInfluence(float multiplier, float changer, BuffInfo realOwner, IDamageInfluencer owner, int priority = 0) : base(owner, priority)
        {
            _hitRateMultiplier = multiplier;
            _realOwner = realOwner;
            _hitRateChanger = changer;
        }

        public override DamageInfluenceType DamageInfluenceType => DamageInfluenceType.HitRate;

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