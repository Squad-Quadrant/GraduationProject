using System.Collections.Generic;
using Core.Log;
using Data.Runtime.Events.Damage;
using Systems.Damage;
using UnityEngine;

namespace Systems.Buff.Config
{
    [CreateAssetMenu(fileName = "AddHPEvent", menuName = "Game/Buff/BuffEvent/AddHPEvent")]
    public class AddHPEvent : UnitBuffEvent
    {
        public int delta;
        // public BuffInfo owner;
        protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
        {
            // owner = buffInfo;
            // var info = new GeneralDamageTriggeringInfo(this, unit);
            // buffInfo.EventBus.Publish(new DealDamageEvent(info));
            
            int newHp = Mathf.Min(unit.CurrentHp + delta, unit.maxHp);
            int actualHeal = newHp - unit.CurrentHp;
            unit.CurrentHp = newHp;
            this.Log($"{unit.name} 回复血量：{actualHeal}，当前血量：{unit.CurrentHp}/{unit.maxHp}", true);
            
        }

        
        // public string DisplayName => owner.Name;

        // public List<Damage.DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        // {
        //     return new() { new GeneralHPInfluence(multiplier, changer, this) };
        // }
    }
}