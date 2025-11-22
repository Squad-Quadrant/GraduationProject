using System;
using Data.Config;
using Systems.Turn;
using UnityEngine;

namespace Systems.Unit
{
	[Serializable]
	public class Unit : ITurnUnit
	{
		public string Id { get; private set; }
		public string ConfigId { get; private set; }
		public string Name { get; private set; }
		public UnitStats Stats { get; private set; } = new();
		public UnitRuntime Runtime { get; private set; } = new();
		public Vector2Int Position { get; set; }

		#region ITurnUnit

		int ITurnUnit.Speed => Stats?.speed ?? 0;
		bool ITurnUnit.CanAct => Runtime is { StillAlive: true, isStunned: false };
		public int ActionPriority { get; set; }

		#endregion
		
		internal static Unit LoadFromConfig(string unitId, UnitConfig config, Vector2Int startPosition)
		{
			var unit = new Unit()
			{
				Id = unitId,
				ConfigId = config.configId,
				Name = config.unitName,
				Stats = config.stats.Clone(),
				Position = startPosition
			};
			unit.Runtime.Initialize(unit.Stats.maxHp);
			return unit;
		}

		public override string ToString() =>
			$"[Unit] {Name}({Id}) HP:{Runtime?.currentHp}/{Stats?.maxHp} Pos:{Position}";
	}
}
