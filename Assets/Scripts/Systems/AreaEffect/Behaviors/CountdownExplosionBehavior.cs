using Core.Log;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class CountdownExplosionBehavior : AreaEffectBehavior
	{
		private readonly int _damage;

		public override string DisplayName { get; }
		public override Sprite DisplayIcon { get; }
		public override bool DestroyOnOwnerDeath => false;

		public CountdownExplosionBehavior(int damage, string displayName, Sprite displayIcon)
		{
			_damage = damage;
			DisplayName = displayName;
			DisplayIcon = displayIcon;
		}

		public override void OnExpired(AreaEffect self, AreaEffectContext ctx)
		{
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
