using System;
using Core.Events;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Damage;
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
        
        // todo: 通用伤害触发事件
        private void DealDamage(UnitAttackedDealDamageEvent e)
        {
            var damageChain = new DamageExecutingChain(DamageType.Bullet);
            DamageExecutingContext context =
                new DamageExecutingContext(e.Attacker, e.Target, e.ActionType, damageChain);
            damageChain.Init(context, _eventBus);
            damageChain.Execute();

            _unitService.CheckUnitDeath();
        }

        public DamageExecutingContext GetSimulatedDamage(DamageTriggeringInfo info)
        {
            var damageChain = new DamageExecutingChain(info.DamageType);
            DamageExecutingContext context =
                new DamageExecutingContext(info.Attacker, info.Defender, info.ActionType, damageChain);
            context.needApplyDamage = false;
            damageChain.Init(context, _eventBus);
            damageChain.Execute();

            return context;
        }
    }

    public struct DamageTriggeringInfo
    {
        public DamageType DamageType;
        public Unit.Unit Attacker;
        public Unit.Unit Defender;
        public EActionType ActionType;
        public DamageTriggeringInfo(DamageType damageType, Unit.Unit attacker, Unit.Unit defender, EActionType actionType)
        {
            DamageType = damageType;
            Attacker = attacker;
            Defender = defender;
            ActionType = actionType;
        }
    }

    public enum DamageType
    {
        Buff,
        Bullet,
        Boom
    }
}