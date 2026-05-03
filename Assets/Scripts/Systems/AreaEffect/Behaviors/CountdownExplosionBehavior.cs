using System.Collections.Generic;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Vfx;
using Systems.Damage;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class CountdownExplosionBehavior : AreaEffectBehavior, IDamageInfluencer
	{
		private readonly int _damage;
		private readonly GameObject _explosionVfxPrefab;

		public CountdownExplosionBehavior(
			int damage,
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null,
			GameObject explosionVfxPrefab = null)
			: base(displayName, displayIcon, persistentVfxPrefab)
		{
			_damage = damage;
			_explosionVfxPrefab = explosionVfxPrefab;
		}

		public override void OnExpired(AreaEffect self, AreaEffectContext ctx)
		{
			ctx.EventBus.PublishOneShotVfx(_explosionVfxPrefab, self.TargetCell);

			// 对覆盖格内所有存活单位造成爆炸伤害
			foreach (var cell in self.Cells)
			{
				var unit = ctx.UnitService.GetUnitAtPosition(cell);
				if (unit is not { IsAlive: true }) continue;

                if (_damage <= 0) continue;
                        
                var info = new GeneralDamageTriggeringInfo(this, unit);
                ctx.EventBus.Publish(new DealDamageEvent(info));
			}
		}

        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            return new List<DamageInfluence>
            {
                new GeneralHPInfluence(1, _damage, this)
            };
        }
    }
}
