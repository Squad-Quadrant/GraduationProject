using System.Collections.Generic;
using UnityEngine;

namespace Systems.AreaEffect
{
	// 地图上持续效果抽象基类，基于类型定义，燃烧 / 定时炸弹 / 侦察眼 等各自派生一个子类
	public abstract class AreaEffectBehavior
	{
		public string DisplayName { get; }
		public Sprite DisplayIcon { get; }
		public GameObject PersistentVfxPrefab { get; }   // 可为 null

		public bool DestroyOnOwnerDeath { get; }
        public IReadOnlyList<Vector2Int> Cells { get; set; }

		protected AreaEffectBehavior(
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null,
			bool destroyOnOwnerDeath = false)
		{
			DisplayName = displayName;
			DisplayIcon = displayIcon;
			PersistentVfxPrefab = persistentVfxPrefab;
			DestroyOnOwnerDeath = destroyOnOwnerDeath;
		}

		public virtual void OnCreated(AreaEffect self, AreaEffectContext ctx) { }
		public virtual void OnUnitEntered(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx) { }
		public virtual void OnUnitLeft(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx) { }
		public virtual void OnUnitTurnStart(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx) { }
		public virtual void OnExpired(AreaEffect self, AreaEffectContext ctx) { }
		public virtual void OnRemoved(AreaEffect self, AreaEffectContext ctx) { }
	}
}
