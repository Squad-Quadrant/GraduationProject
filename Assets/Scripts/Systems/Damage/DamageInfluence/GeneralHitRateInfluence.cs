using System.Collections.Generic;
using Systems.Buff;

namespace Systems.Damage
{
    public class GeneralHitRateInfluence : DamageInfluence
    {
        private float _hitRateMultiplier;
        private float _hitRateChanger;
        public string DisplayName;
        public GeneralHitRateInfluence(float multiplier, float changer, string displayName, IDamageInfluencer owner, int priority = 0) : base(owner, priority)
        {
            _hitRateMultiplier = multiplier;
            DisplayName = displayName;
            _hitRateChanger = changer;
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.HitRate, DamageInfluenceType.Buff };

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
            Context.AddHitRateInfluence(DisplayName, _hitRateChanger, _hitRateMultiplier);
        }

        public override void Execute()
        {

        }

        public override void Last()
        {
            
        }
    }
}