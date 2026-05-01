using System;
using System.Collections.Generic;
using System.Linq;
using Core.Commands;
using Core.Log;
using DG.Tweening;
using Systems.AreaEffect.Behaviors;
using Systems.Interaction;
using Systems.Interaction.Targeting;
using UnityEngine;

namespace Systems.Unit.Skill.Logic
{
	public class ScoutEyeSkillLogic : SkillLogic, ITargeted
	{
		public ScoutEyeSkillLogic(SkillConfig config, Unit owner) : base(config, owner)
		{
			if (config.kind != ESkillKind.ScoutEye)
				throw new ArgumentException(
					$"SkillConfig kind mismatch: expected ScoutEye, got {config.kind}",
					nameof(config));
		}

		public IReadOnlyList<Vector2Int> GetValidCells(InteractionContext ctx)
		{
			var result = new List<Vector2Int>();
			var origin = Owner.position;
			int r = Config.range;

			for (int dx = -r; dx <= r; dx++)
			{
				int maxDy = r - Mathf.Abs(dx);
				for (int dy = -maxDy; dy <= maxDy; dy++)
				{
					var cell = origin + new Vector2Int(dx, dy);
					if (!ctx.MapService.Data.IsInBounds(cell)) continue;
					result.Add(cell);
				}
			}
			return result;
		}

		public bool ValidateTarget(Vector2Int cell, InteractionContext ctx) =>
			ctx.VisionCalculator.TraceRay(Owner.position, cell);

		public IReadOnlyList<Vector2Int> GetAreaEffectPreview(Vector2Int hoverCell) =>
			ExpandCoverage(hoverCell);

		public ICommand CreateCommand(Vector2Int target, InteractionContext ctx)
		{
			var cells = ExpandCoverage(target);
			var behavior = new ScoutEyeBehavior(
				visionRadius: Config.visionReach,
				displayName:  Config.skillName,
				displayIcon:  Config.icon,
				persistentVfxPrefab: Config.persistentVfxPrefab);

			return new AsyncLambdaCommand(
				$"Skill/ApplyAreaEffect({Owner.name} → {target}, {behavior.DisplayName}, persist={Config.persistTurns})",
				onComplete =>
				{
					Owner.CurrentAp -= Config.apCost;
					Consume();

					// todo: 需要动画或者反馈
					DOVirtual.DelayedCall(0.2f, () =>
					{
						var effect = ctx.AreaEffectService.Register(
							ownerId:        Owner.id,
							targetCell:     target,
							cells:          cells,
							remainingTurns: Config.persistTurns,
							behavior:       behavior);

						this.Log($"Registered {effect}");
						onComplete();
					});
				});
		}

		private List<Vector2Int> ExpandCoverage(Vector2Int center)
		{
			var offsets = Config.coverageOffsets;
			if (offsets == null || offsets.Length == 0)
				return new List<Vector2Int> { center }; // 兜底：至少覆盖落点

			var result = new List<Vector2Int>(offsets.Length);
			result.AddRange(offsets.Select(o => center + o));
			return result;
		}
	}
}
