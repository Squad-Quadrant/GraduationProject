using System.Collections.Generic;
using Core.Commands;
using Core.Log;
using Data.Runtime.Commands;
using Systems.AreaEffect.Behaviors;
using Systems.Interaction;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
{
	// 投掷类战术道具的基类 Logic
	public class ThrowableGrenadeLogic : TacticalItemLogic, ITargeted
	{
		public ThrowableGrenadeLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public virtual IReadOnlyList<Vector2Int> GetValidCells(InteractionContext ctx)
		{
			var result = new List<Vector2Int>();
			var origin = Owner.position;
			int r = ItemConfig.throwRange;

			// 遍历曼哈顿距离 ≤ r 的所有格子
			for (int dx = -r; dx <= r; dx++)
			{
				int maxDy = r - Mathf.Abs(dx);
				for (int dy = -maxDy; dy <= maxDy; dy++)
				{
					var cell = origin + new Vector2Int(dx, dy);
					if (!ctx.MapService.Data.IsInBounds(cell)) continue;

					// if (!ctx.VisionCalculator.TraceRay(origin, cell)) continue;

					result.Add(cell);
				}
			}
			return result;
		}

		public virtual bool ValidateTarget(Vector2Int cell, InteractionContext ctx) =>
			ctx.VisionCalculator.TraceRay(Owner.position, cell);

		public IReadOnlyList<Vector2Int> GetAreaEffectPreview(Vector2Int hoverCell) =>
			ExpandCoverage(hoverCell);

		public virtual ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
			new ThrowInstantAoECommand(
				logic:       this,
				apCost:      ItemConfig.apCost,
				targetCell:  target,
				aoeCells:    ExpandCoverage(target),
				damage:      ItemConfig.directDamage,
				unitService: ctx.UnitService);
	}

	// 燃烧弹
	public class ThrowableBurnLogic : ThrowableGrenadeLogic
	{
		public ThrowableBurnLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx)
		{
			var behavior = new AttachBuffAreaBehavior(
				buffType:    ItemConfig.appliedBuff,
				displayName: ItemConfig.nName,
				displayIcon: ItemConfig.icon);

			return new ThrowAreaEffectCommand(
				logic:             this,
				apCost:            ItemConfig.apCost,
				targetCell:        target,
				cells:             ExpandCoverage(target),
				persistTurns:      ItemConfig.persistTurns,
				behavior:          behavior,
				areaEffectService: ctx.AreaEffectService);
		}
	}

	// 定时炸弹
	public class ThrowableTimerBombLogic : ThrowableGrenadeLogic
	{
		public ThrowableTimerBombLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx)
		{
			var behavior = new CountdownExplosionBehavior(
				damage:      ItemConfig.directDamage,
				displayName: ItemConfig.nName,
				displayIcon: ItemConfig.icon);

			return new ThrowAreaEffectCommand(
				logic:             this,
				apCost:            ItemConfig.apCost,
				targetCell:        target,
				cells:             ExpandCoverage(target),
				persistTurns:      ItemConfig.persistTurns,
				behavior:          behavior,
				areaEffectService: ctx.AreaEffectService);
		}
	}

	// 侦察眼
	public class ThrowableScoutEyeLogic : ThrowableGrenadeLogic
	{
		public ThrowableScoutEyeLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx)
		{
			var behavior = new ScoutEyeBehavior(
				visionRadius: ItemConfig.visionReach,
				displayName:  ItemConfig.nName,
				displayIcon:  ItemConfig.icon);

			return new ThrowAreaEffectCommand(
				logic:             this,
				apCost:            ItemConfig.apCost,
				targetCell:        target,
				cells:             ExpandCoverage(target),
				persistTurns:      ItemConfig.persistTurns,
				behavior:          behavior,
				areaEffectService: ctx.AreaEffectService);
		}
	}
}
