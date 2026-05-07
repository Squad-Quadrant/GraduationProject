using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
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
            
            _eventBus.Subscribe<DealDamageEvent>(DealDamage);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<DealDamageEvent>(DealDamage);
            this.Log("Disposed");
        }
        
        private void DealDamage(DealDamageEvent e)
        {
            var damageChain = GetDamageChain(e.Info);
            damageChain.Init();
            damageChain.Execute();

            _unitService.CheckUnitDeath();
        }

        public DamageExecutingContext GetSimulatedDamage(BulletDamageTriggeringInfo info, 
            out Dictionary<BodyPartType, DamageExecutingContext> simulatedBodyPartDamages)
        {
            simulatedBodyPartDamages = new();
            
            var damageChain = GetDamageChain(info);
            damageChain.Context.IsSimulating = true;
            damageChain.Context.needApplyDamage = false;
            damageChain.Init();
            damageChain.Execute();

            foreach (var bodyPart in info.Defender.BodyPartInfo.Keys)
            {
                var bodyPartDamageChain = GetDamageChain(info);
                bodyPartDamageChain.Context.IsSimulating = true;
                bodyPartDamageChain.Context.needApplyDamage = false;
                bodyPartDamageChain.Context.bodyPartType = bodyPart;
                bodyPartDamageChain.Init();
                bodyPartDamageChain.Execute();
                simulatedBodyPartDamages[bodyPart] = bodyPartDamageChain.Context;
            }

            return damageChain.Context;
        }
        
        public DamageExecutingChain GetDamageChain(DamageTriggeringInfo info)
        {
            var context = new DamageExecutingContext(info);
            DamageExecutingChain damageChain;
            switch (info.DamageType)
            {
                case DamageType.General:
                    return new GeneralDamageExecutingChain(context, _eventBus);
                
                case DamageType.Bullet:
                    var bulletInfo = info as BulletDamageTriggeringInfo;
                    context.ActionType = bulletInfo.ActionType;
                    damageChain = new BulletDamageExecutingChain(context, _eventBus);
                    return damageChain;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(info.DamageType), info.DamageType, null);
            }
        }

    }
    
    public enum DamageType
    {
        General,
        Bullet,
    }
}