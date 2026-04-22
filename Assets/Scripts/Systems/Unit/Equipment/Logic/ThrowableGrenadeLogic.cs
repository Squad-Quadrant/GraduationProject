using System.Collections.Generic;
using Core.Commands;
using Core.Log;
using DG.Tweening;
using Systems.AreaEffect;
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

		public virtual ICommand CreateCommand(Vector2Int target, InteractionContext ctx)
		{
			var aoeCells = ExpandCoverage(target);
			var damage = ItemConfig.directDamage;

			return new AsyncLambdaCommand(
				$"ThrowAoE({Owner.name} → {target}, dmg={damage}, cells={aoeCells.Count})",
				onComplete =>
				{
					Owner.CurrentAp -= ItemConfig.apCost;
					Consume();

					DOVirtual.DelayedCall(0.2f, () => // todo: 需要动画或者反馈
					{
						foreach (var cell in aoeCells)
						{
							var unit = ctx.UnitService.GetUnitAtPosition(cell);
							if (unit is not { IsAlive: true }) continue;
							// TODO(Damage): 等 IDamageService 扩展后替换为实际伤害结算
							this.Log($"[TODO(Damage)] Grenade @{cell}: would deal {damage} to '{unit.name}'");
						}
						onComplete();
					});
				});
		}

		protected ICommand BuildAreaEffectCommand(
			Vector2Int target,
			AreaEffectBehavior behavior,
			InteractionContext ctx)
		{
			var cells = ExpandCoverage(target);

			return new AsyncLambdaCommand(
				$"ThrowAreaEffect({Owner.name} → {target}, {behavior.DisplayName}, persist={ItemConfig.persistTurns})",
				onComplete =>
				{
					Owner.CurrentAp -= ItemConfig.apCost;
					Consume();

					DOVirtual.DelayedCall(0.2f, () => // todo: 需要动画或者反馈
					{
						var effect = ctx.AreaEffectService.Register(
							ownerId:        Owner.id,
							targetCell:     target,
							cells:          cells,
							remainingTurns: ItemConfig.persistTurns,
							behavior:       behavior);

						this.Log($"Registered {effect}");
						onComplete();
					});
				});
		}
	}

	// 燃烧弹
	public class ThrowableBurnLogic : ThrowableGrenadeLogic
	{
		public ThrowableBurnLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
			BuildAreaEffectCommand(
				target,
				new AttachBuffAreaBehavior(
					buffType:    ItemConfig.appliedBuff,
					displayName: ItemConfig.nName,
					displayIcon: ItemConfig.icon),
				ctx);
	}

	// 定时炸弹
	public class ThrowableTimerBombLogic : ThrowableGrenadeLogic
	{
		public ThrowableTimerBombLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
			BuildAreaEffectCommand(
				target,
				new CountdownExplosionBehavior(
					damage:      ItemConfig.directDamage,
					displayName: ItemConfig.nName,
					displayIcon: ItemConfig.icon),
				ctx);
	}

	// 侦察眼
	public class ThrowableScoutEyeLogic : ThrowableGrenadeLogic
	{
		public ThrowableScoutEyeLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
			BuildAreaEffectCommand(
				target,
				new ScoutEyeBehavior(
					visionRadius: ItemConfig.visionReach,
					displayName:  ItemConfig.nName,
					displayIcon:  ItemConfig.icon),
				ctx);
	}
}
