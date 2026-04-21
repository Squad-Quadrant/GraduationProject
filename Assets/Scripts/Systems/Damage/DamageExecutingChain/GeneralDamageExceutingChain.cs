using Core.Events;

namespace Systems.Damage
{
    public class GeneralDamageExecutingChain : DamageExecutingChain
    {
        public GeneralDamageExecutingChain(DamageExecutingContext context, IEventBus eventBus) : base(context, eventBus)
        {
        }

        public override DamageType DamageType => DamageType.General;
        protected override void InitInfluencers()
        {
            
        }

        public override void Execute()
        {
            
        }
    }
}