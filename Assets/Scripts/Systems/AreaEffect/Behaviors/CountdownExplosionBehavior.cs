using Core.Log;
using Data.Runtime.Events.Vfx;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class CountdownExplosionBehavior : AreaEffectBehavior
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

				// TODO(DamageService)
				this.Log($"[TODO(DamageService)] Explosion of {self.Id} at {cell}: would deal {_damage} damage to {unit.name}");
			}
		}
	}
}
