using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Commands;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Map;
using Data.Runtime.Events.UI;
using Systems.Damage;
using Systems.Map;
using Systems.Map.Config;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class AttackPreviewState : InteractionState
	{
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<CellClickedEvent> _onCellClicked;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;
		private Action<PointerHoverEvent> _onPointerHover;

		private IReadOnlyList<Vector2Int> _validTargetCells;

		private static readonly Vector2Int[] Directions =
		{
			Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
		};

        public AttackPreviewState() : base(InteractionStates.AttackPreview) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			if (ctx.selectedUnit == null)
				throw new InvalidOperationException("No unit selected when entering AttackPreviewState.");

			this.Log($"Entered - Unit: {ctx.selectedUnit.name}");

			_validTargetCells = ctx.selectedUnit.CalculateSelectableTargets(ctx.UnitService, ctx.VisionService).Select(u => u.position).ToList();
            
			Publish(ctx, new RangeDisplayEvent(
				ERangeType.Attack,
				_validTargetCells,
				origin: ctx.selectedUnit.position,
				sourceUnitId: ctx.selectedUnit.id));

			_onUnitClicked = OnUnitClicked;
			_onCellClicked = OnCellClicked;
			_onBack = OnBack;
			_onEsc = OnEsc;
			_onPointerHover = OnPointerHover;

			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
			Subscribe(ctx, _onPointerHover);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Attack));
			Publish(ctx, PathPreviewEvent.Hide());
			Publish(ctx, CursorInfoEvent.Hide());
			Publish(ctx, TargetingEvent.Clear());
            Publish(Context, new RemoveGunLineEvent());
            Publish(ctx, DisplayAttackContextEvent.Invalid());
            
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);
			Unsubscribe(ctx, _onPointerHover);

			_onUnitClicked = null;
			_onCellClicked = null;
			_onBack = null;
			_onEsc = null;
			_onPointerHover = null;

			_validTargetCells = null;

			base.OnExit(ctx);
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (!Context.UnitService.TryGetUnit(e.UnitId, out var target))
			{
				this.LogError($"invalid unit {e.UnitId}.");
				return;
			}
			TryAttackTarget(target);
		}

		private void OnCellClicked(CellClickedEvent e)
		{
			var target = Context.UnitService.GetUnitAtPosition(e.CellPosition);
			if (target == null) return;
			TryAttackTarget(target);
		}

		private void TryAttackTarget(Unit.Unit target)
		{
			if (!_validTargetCells.Contains(target.position))
			{
				this.Log($"Target {target.name} at {target.position} not in valid cells, ignored");
				return;
			}

			Context.VisionCalculator.TraceRay(Context.selectedUnit.position, target.position, out var info);
			if (!info.CanGunLinePass())
			{
				this.Log($"Gun line blocked to {target.name}, ignored");
				return;
			}

			ExecuteAttack(target);
		}

		private void OnBack(BackInputEvent e)
		{
			this.Log("Back -> UnitSelected");
			Context.StateMachine.ChangeState<UnitSelectedState>();
		}

		private void OnEsc(EscInputEvent e)
		{
			this.Log("ESC → UnitSelected");
			Context.StateMachine.ChangeState<UnitSelectedState>();
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			if (!e.CellPosition.HasValue && string.IsNullOrEmpty(e.HoveredUnitId))
			{
				Publish(Context, CursorInfoEvent.Hide());
				return;
			}

			var hoveredTarget = ResolveHoveredAttackable(e);

			if (hoveredTarget == null)
			{
				PublishBasicCursorInfo(Context, e);
				return;
			}

			Context.VisionCalculator.TraceRay(Context.selectedUnit.position, hoveredTarget.position, out var visionInfo);
			if (!visionInfo.CanGunLinePass())
			{
				PublishBasicCursorInfo(Context, e);
				return;
			}

			ShowAttackPreview(hoveredTarget, e.WorldPosition);
		}

		private Unit.Unit ResolveHoveredAttackable(PointerHoverEvent e)
		{
			if (e.HoveredUnitId != null &&
			    Context.UnitService.TryGetUnit(e.HoveredUnitId, out var unit) &&
			    _validTargetCells.Contains(unit.position))
				return unit;

			if (e.CellPosition.HasValue &&
			    _validTargetCells.Contains(e.CellPosition.Value))
				return Context.UnitService.GetUnitAtPosition(e.CellPosition.Value);

			return null;
		}

		private void ExecuteAttack(Unit.Unit target)
		{
            this.Log($"Executing attack on target: {target.name}");

			var attacker = Context.selectedUnit;
			var attackCommand = new UnitAttackCommand(
				attacker.id,
                target.id,
				1,
                Context.currentAction,
				Context.UnitService,
				Context.MapService,
				Context.EventBus,
				Context.AudioService
			);
			
			Context.CommandQueue.EnqueueAndExecute(attackCommand);
            Context?.StateMachine.ChangeState<ExecutingState>();
		}

		private void ShowAttackPreview(Unit.Unit target, Vector3 cursorWorldPosition)
		{
			var damageContext = ComputeDamageContext(target, out var contextDic, out var lowWallKeys);
			int hitPercent = Mathf.Clamp(Mathf.RoundToInt(damageContext.HitRate * 100f), 0, 100);

			Publish(Context, CursorInfoEvent.ForAttack(
				target.position, cursorWorldPosition,
				hitPercent, target.name, target.CurrentHp, target.maxHp));

			PublishAttackPreviewEvents(target, damageContext, contextDic, lowWallKeys);
		}

		private void PublishAttackPreviewEvents(
			Unit.Unit target,
			DamageExecutingContext damageContext,
			Dictionary<BodyPartType, DamageExecutingContext> contextDic,
			List<WallKey> lowWallKeys)
		{
			if (target.faction != EUnitFaction.Player)
				Publish(Context, new UpdateGunLineEvent(Context.selectedUnit, target, lowWallKeys));

			Publish(Context, DisplayAttackContextEvent.Valid(damageContext, Context.selectedUnit.id, contextDic));
		}

		private DamageExecutingContext ComputeDamageContext(
			Unit.Unit target,
			out Dictionary<BodyPartType, DamageExecutingContext> contextDic,
			out List<WallKey> lowWallKeys)
		{
			var environment = new List<IDamageInfluencer>();
			lowWallKeys = new List<WallKey>();

			Context.VisionCalculator.TraceRay(Context.selectedUnit.position, target.position, out var info);
			var mapData = Context.MapService.Data;

			foreach (var dir in Directions)
			{
				var key = new WallKey(target.position, target.position + dir);
				var wall = mapData.GetWall(key);
				if (wall is not { Type: WallType.LowWall }) continue;
				if (!info.lowWalls.Contains(key)) continue;
				if (environment.Contains(wall)) continue;

				environment.Add(wall);
				lowWallKeys.Add(key);
			}

			return Context.DamageService.GetSimulatedDamage(
				new BulletDamageTriggeringInfo(
					Context.selectedUnit,
					target,
					Context.currentAction,
					environment),
				out contextDic);
		}

	}
}
