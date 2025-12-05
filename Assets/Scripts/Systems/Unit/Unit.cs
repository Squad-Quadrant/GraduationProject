using System;
using System.Collections.Generic;
using Data.Config;
using Data.Runtime;
using Sirenix.OdinInspector;
using Systems.Turn;
using UnityEngine;

namespace Systems.Unit
{
	[Serializable]
	public class Unit : ITurnUnit
	{
		[ReadOnly] public string id;
		[ReadOnly] public string configId;
		[ReadOnly] public string name;
		[ReadOnly] public UnitStats stats = new();
		[ReadOnly] public UnitRuntime runtime = new();
		[ReadOnly] public Vector2Int position;

		#region ITurnUnit

		string ITurnUnit.Id => id;
		int ITurnUnit.Speed => stats?.speed ?? 0;
		bool ITurnUnit.CanAct => runtime is { StillAlive: true, isStunned: false };
		public int ActionPriority { get; set; }

		#endregion
		
		internal static Unit LoadFromConfig(string unitId, UnitConfig config, Vector2Int startPosition)
		{
			var unit = new Unit()
			{
				id = unitId,
				configId = config.configId,
				name = config.unitName,
				stats = config.stats.Clone(),
				position = startPosition
			};
			unit.runtime.Initialize(unit.stats.maxHp);
			return unit;
		}

		public List<EActionType> GetAvailableActions()
		{
			// todo: need to calculate based on unit state, abilities, etc.
			var actions = new List<EActionType>
			{
				EActionType.Move,
				EActionType.Attack,
				EActionType.Wait,
				EActionType.EndTurn
			};

			return actions;
		}

		public override string ToString() =>
			$"[Unit] {name}({id}) HP:{runtime?.currentHp}/{stats?.maxHp} Pos:{position}";
	}
}
