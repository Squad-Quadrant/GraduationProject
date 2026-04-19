using System.Collections.Generic;
using System.Linq;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
{
	public abstract class TacticalItemLogic : EquipmentLogic
	{
		protected readonly TacticalItemConfig ItemConfig;

		public int RemainingUses { get; private set; }

		public bool CanUse => RemainingUses > 0;

		protected TacticalItemLogic(TacticalItemConfig itemConfig, Unit owner) : base(itemConfig, owner)
		{
			ItemConfig = itemConfig;
			RemainingUses = itemConfig.maxUsesPerBattle;
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
			var result = new List<Vector2Int>(offsets?.Length ?? 1);
			if (offsets == null || offsets.Length == 0)
			{
				result.Add(center);   // 兜底：至少覆盖落点自身
				return result;
			}

			result.AddRange(offsets.Select(o => center + o));
			return result;
		}
	}
}
