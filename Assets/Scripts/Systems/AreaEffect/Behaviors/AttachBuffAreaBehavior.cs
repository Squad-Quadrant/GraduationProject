using Core.Log;
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class AttachBuffAreaBehavior : AreaEffectBehavior
	{
		private readonly BuffType _buffType;

		public AttachBuffAreaBehavior(
			BuffType buffType,
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null)
			: base(displayName, displayIcon, persistentVfxPrefab)
		{
			_buffType = buffType;
		}

		public override void OnUnitEntered(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx)
		{
			// TODO(Buff): BuffService 接口完善后接入
			// unit.AttachBuff(_buffType, creator: self);
			this.Log($"[TODO(Buff)] {unit.name} entered {self.Behavior.DisplayName} at {cell}, would attach {_buffType}");
		}

		public override void OnUnitTurnStart(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx)
		{
			// TODO(Buff): BuffService 接口完善后接入
			// unit.AttachBuff(_buffType, creator: self);
			this.Log($"[TODO(Buff)] {unit.name} turn started inside {self.Behavior.DisplayName} at {cell}, would attach {_buffType}");
		}
	}
}
