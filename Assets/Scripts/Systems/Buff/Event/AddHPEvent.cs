using System.Collections.Generic;
using Core.Log;
using Data.Runtime.Events.Damage;
using Systems.Damage;
using UnityEngine;

namespace Systems.Buff.Config
{
    [CreateAssetMenu(fileName = "AddHPEvent", menuName = "Game/Buff/BuffEvent/AddHPEvent")]
    public class AddHPEvent : UnitBuffEvent, IDamageInfluencer
    {
        public int delta;
        public BuffInfo owner; // todo: 为BuffEvent统一添加owner
        protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
        {
            // int newHp = Mathf.Min(unit.CurrentHp + delta, unit.maxHp);
            // int actualHeal = newHp - unit.CurrentHp;
            // unit.CurrentHp = newHp;
            // this.Log($"{unit.name} 回复血量：{actualHeal}，当前血量：{unit.CurrentHp}/{unit.maxHp}", true);
            owner = buffInfo;
            buffInfo.EventBus.Publish(new DealDamageEvent(new RecoverTriggeringInfo(this, unit, delta)));
        }

        public string DisplayName => owner.Name;
        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            return null;
        }
    }
}