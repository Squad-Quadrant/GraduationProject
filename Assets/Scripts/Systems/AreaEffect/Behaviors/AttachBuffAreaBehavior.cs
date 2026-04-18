using Core.Log;
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class AttachBuffAreaBehavior : AreaEffectBehavior
	{
		private readonly BuffType _buffType;

		public override string DisplayName { get; }
		public override Sprite DisplayIcon { get; }

		public override bool DestroyOnOwnerDeath => false;

		public AttachBuffAreaBehavior(BuffType buffType, string displayName, Sprite displayIcon)
		{
			_buffType = buffType;
			DisplayName = displayName;
			DisplayIcon = displayIcon;
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
