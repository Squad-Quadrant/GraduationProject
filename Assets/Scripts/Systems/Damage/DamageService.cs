using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Systems.Unit;

namespace Systems.Damage
{
    public class DamageService : IDamageService, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IUnitService _unitService;

        public DamageService(IEventBus eventBus, IUnitService unitService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            this.Log("Initialized");
            
            _eventBus.Subscribe<UnitAttackedDealDamageEvent>(DealDamage);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<UnitAttackedDealDamageEvent>(DealDamage);
            this.Log("Disposed");
        }
        
        private void DealDamage(UnitAttackedDealDamageEvent e)
        {
            var damageChain = new DamageExecutingChain(DamageType.Bullet);
            DamageExecutingContext context =
                new DamageExecutingContext(e.Attacker, e.Target, e.ActionType, damageChain);
            damageChain.Init(context);
            damageChain.Execute();

            _unitService.CheckUnitDeath();
        }
    }

    public enum DamageType
    {
        Buff,
        Bullet,
        Boom
    }
}