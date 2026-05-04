using System.Collections.Generic;
using Presentation.Audio;
using Presentation.Bootstrap;
using Systems.PathFinding;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
{
	public abstract class TacticalItemLogic : EquipmentLogic
	{
		protected readonly TacticalItemConfig ItemConfig;

		protected PathFindingOptions PathFindingOptions;

		private IPathFindingService _pathFindingService;
		protected IPathFindingService PathFindingService => _pathFindingService ??= LevelContainer.Instance.Resolve<IPathFindingService>();

		private AudioService _audioService;
		protected AudioService AudioService => _audioService ??= RootContainer.Instance.Resolve<AudioService>();

		private static readonly Vector2Int[] BfsDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

		public int RemainingUses { get; private set; }

		public virtual bool CanUse => RemainingUses > 0 && Owner.CurrentAp >= ItemConfig.apCost;

		protected TacticalItemLogic(TacticalItemConfig itemConfig, Unit owner) : base(itemConfig, owner)
		{
			ItemConfig = itemConfig;
			RemainingUses = itemConfig.maxUsesPerBattle;
			PathFindingOptions = new PathFindingOptions(
				canPassThroughAllies: true,
				enemiesBlockMovement: false,
				movingUnitFaction: Owner.faction,
				movingUnitId: Owner.id,
				canCrossHighWalls: false,
				canCrossLowWalls: false,
				ignoreTerrainWalkability: true);
		}

		public void Consume()
		{
			if (RemainingUses <= 0) return;
			RemainingUses--;
			Owner.TriggerInfoChanged();
		}

		// 战术道具不参与传统攻击判定（不走 Attack 链路），占位满足基类契约
		public override int Range() => 0;
		public override bool CheckAttackable(Unit target) => false;

		protected List<Vector2Int> ExpandCoverage(Vector2Int center)
		{
			var offsets = ItemConfig.coverageOffsets;
			if (offsets == null || offsets.Length == 0)
				return new List<Vector2Int> { center };

			var candidates = new HashSet<Vector2Int>(offsets.Length);
			foreach (var offset in offsets)
				candidates.Add(center + offset);

			var result = new List<Vector2Int>(offsets.Length);
			var visited = new HashSet<Vector2Int> { center };
			var queue = new Queue<Vector2Int>();
			queue.Enqueue(center);

			if (candidates.Contains(center))
				result.Add(center);

			while (queue.Count > 0)
			{
				var current = queue.Dequeue();
				foreach (var direction in BfsDirections)
				{
					var next = current + direction;
					if (!candidates.Contains(next)) continue;
					if (!visited.Add(next)) continue;
					if (!PathFindingService.CanTraverseBetween(current, next, PathFindingOptions)) continue;
					queue.Enqueue(next);
					result.Add(next);
				}
			}
			return result;
		}
	}
}
