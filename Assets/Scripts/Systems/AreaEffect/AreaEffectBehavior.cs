using UnityEngine;

namespace Systems.AreaEffect
{
	// 地图上持续效果抽象基类，基于类型定义，燃烧 / 定时炸弹 / 侦察眼 等各自派生一个子类
	public abstract class AreaEffectBehavior
	{
		public abstract string DisplayName { get; }
		public abstract Sprite DisplayIcon { get; }

		public virtual GameObject VfxPrefab => null;

		public virtual bool DestroyOnOwnerDeath => false;

		public virtual void OnCreated(AreaEffect self, AreaEffectContext ctx) { }
		public virtual void OnUnitEntered(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx) { }
		public virtual void OnUnitTurnStart(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx) { }
		public virtual void OnExpired(AreaEffect self, AreaEffectContext ctx) { }
		public virtual void OnRemoved(AreaEffect self, AreaEffectContext ctx) { }
	}
}
