using System.Collections.Generic;
using Data.Runtime.Events.Damage;
using Systems.Damage;
using UnityEngine;

namespace Systems.Buff.Config
{
	[CreateAssetMenu(fileName = "PercentHp", menuName = "Game/Buff/BuffEvent/PercentHp")]
	public class HPInfluence : UnitBuffEvent, IDamageInfluencer
	{
        public int changer = 0;
        public float multiplier = 1;
        public BuffInfo owner;
		protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
        {
            owner = buffInfo;
            var info = new GeneralDamageTriggeringInfo(this, unit);
			buffInfo.EventBus.Publish(new DealDamageEvent(info));
		}

        public string DisplayName => owner.Name;

        public List<Damage.DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            return new() { new GeneralHPInfluence(multiplier, changer, this) };
        }
    }
}
