using System;
using Data.Config;
using Systems.Turn;
using UnityEngine;

namespace Systems.Unit
{
	[Serializable]
	public class Unit : ITurnUnit
	{
		public string Id { get; set; }
		public string configId; // 指向UnitConfig的ID
		public string name;
		public UnitStats stats = new();		// 角色属性
		public UnitRuntime runtime = new();	// 角色运行时状态
		public Vector2Int position;

		#region ITurnUnit

		int ITurnUnit.Speed => stats?.speed ?? 0;
		bool ITurnUnit.CanAct => runtime is { StillAlive: true, isStunned: false };
		public int ActionPriority { get; set; }

		#endregion

		internal static Unit LoadFromConfig(string unitId, UnitConfig config, Vector2Int startPosition)
		{
			var unit = new Unit()
			{
				Id = unitId,
				configId = config.configId,
				name = config.unitName,
				stats = config.stats.Clone(),
				position = startPosition
			};
			unit.runtime.Initialize(unit.stats.maxHp);
			return unit;
		}

		public override string ToString() =>
			$"[Unit] {name}({Id}) HP:{runtime?.currentHp}/{stats?.maxHp} Pos:{position}";
	}
}
