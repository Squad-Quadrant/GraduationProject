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
		private Action<TargetConfirmEvent> _onTargetConfirm;
		private Action<ActionSelectedEvent> _onActionSelected;

		private IReadOnlyList<Vector2Int> _validTargetCells;
		private Unit.Unit _target;
		private bool hasConfirmed;
		

        public AttackPreviewState() : base(InteractionStates.AttackPreview) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			if (ctx.selectedUnit == null)
				throw new InvalidOperationException("No unit selected when entering AttackPreviewState.");

			this.Log($"Entered - Unit: {ctx.selectedUnit.name}");

			_validTargetCells = CalculateAttackableTarget(ctx).Select(u => u.position).ToList();
            
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
			_onTargetConfirm = OnTargetConfirm;
			_onActionSelected = OnActionSelected;

			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
			Subscribe(ctx, _onPointerHover);
			Subscribe(ctx, _onTargetConfirm);
			Subscribe(ctx, _onActionSelected);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Attack));
			Publish(ctx, PathPreviewEvent.Hide());
			Publish(ctx, CursorInfoEvent.Hide());
			Publish(ctx, TargetingEvent.Clear());
            Publish(Context, new RemoveGunLineEvent());
            
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);
			Unsubscribe(ctx, _onPointerHover);
			Unsubscribe(ctx, _onTargetConfirm);
			Unsubscribe(ctx, _onActionSelected);

			_onUnitClicked = null;
			_onCellClicked = null;
			_onBack = null;
			_onEsc = null;
			_onPointerHover = null;
			_onTargetConfirm = null;
			_onActionSelected = null;

			_validTargetCells = null;
			_target = null;

			base.OnExit(ctx);
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (!Context.UnitService.TryGetUnit(e.UnitId, out var target) ||
			    !_validTargetCells.Contains(target.position))
			{
				this.LogError($"invalid unit {e.UnitId}.");
				return;
			}

			_target = target;
			hasConfirmed = true;
			Publish(Context, new TargetingEvent(target.position));
		}

		private void OnCellClicked(CellClickedEvent e)
		{
			var target = Context.UnitService.GetUnitAtPosition(e.CellPosition);
			if (target == null) return;

			_target = target;
			hasConfirmed = true;
			Publish(Context, new TargetingEvent(target.position));
		}

		private void OnBack(BackInputEvent e)
		{
			if (_target != null)
			{
				this.Log($"Back -> clear target: {_target.name}");
				_target = null;
				Publish(Context, TargetingEvent.Clear());
				hasConfirmed = false;
				Publish(Context, new RemoveGunLineEvent());
				return;
			}
			
			this.Log("Back -> UnitSelected");
			CancelPreview();
			Context.StateMachine.ChangeState<UnitSelectedState>();
		}

		private void OnEsc(EscInputEvent e)
		{
			this.Log("ESC → UnitSelected");
			CancelPreview();
			Context.StateMachine.ChangeState<UnitSelectedState>();
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			if (!e.CellPosition.HasValue && string.IsNullOrEmpty(e.HoveredUnitId))
			{
				Publish(Context, CursorInfoEvent.Hide());
				return;
			}

			Unit.Unit target = null;
			if (e.HoveredUnitId != null
			    && Context.UnitService.TryGetUnit(e.HoveredUnitId, out target)
			    && _validTargetCells.Contains(target.position))
			{
			}
			else if (e.CellPosition.HasValue && _validTargetCells.Contains(e.CellPosition.Value))
			{
				target = Context.UnitService.GetUnitAtPosition(e.CellPosition.Value);
			}

			if (target != null)
			{
				List<IDamageInfluencer> environment = new();
				
				var info = new TraceRayInfo();
				Context.VisionCalculator.TraceRay(Context.selectedUnit.position, target.position, out info);
				var mapData = Context.MapService.Data;

				foreach (var wallKey in info.lowWalls)
				{
					var theWall = mapData.GetWall(wallKey);
					environment.Add(theWall);
					// 仅添加一个墙体作为环境影响因素，避免重复计算同一墙体的穿透效果
					break;
				}
				
				var damageContext = Context.DamageService.GetSimulatedDamage(
					new BulletDamageTriggeringInfo(
						Context.selectedUnit,
						target,
						Context.currentAction,
						environment));

				int hitPercent = Mathf.RoundToInt(damageContext.HitRate * 100f);
				hitPercent = Mathf.Clamp(hitPercent, 0, 100);

				Publish(Context, CursorInfoEvent.ForAttack(
					target.position, e.WorldPosition,
					hitPercent, target.name, target.CurrentHp, target.maxHp));
				Publish(Context, DisplayAttackContextEvent.Valid(damageContext, Context.selectedUnit.id));
				if (!hasConfirmed)
				{
					Publish(Context, new UpdateGunLineEvent(Context.selectedUnit, target, info.lowWalls));
				}
			}
			else
			{
				PublishBasicCursorInfo(Context, e);
				Publish(Context, DisplayAttackContextEvent.Invalid());
				if (!hasConfirmed)
				{
					Publish(Context, new RemoveGunLineEvent());
				}
			}
		}

		private void OnTargetConfirm(TargetConfirmEvent e)
		{
			if (_target == null)
			{
				this.LogError("Target Confirm, but target is null");
				return;
			}

			ExecuteAttack(_target);
		}

		private void OnActionSelected(ActionSelectedEvent e)
		{
			if (e.ActionType != EActionType.Back)
			{
				this.LogError($"Unexcepted actionType: {e.ActionType}");
				return;
			}

			this.Log("Back -> UnitSelected");
			CancelPreview();
			Context.StateMachine.ChangeState<UnitSelectedState>();
		}

		private List<Unit.Unit> CalculateAttackableTarget(InteractionContext ctx)
		{
			var unit = ctx.selectedUnit;

            var currentEquipment = unit.CurrentWeaponContainer;
            
            // 搜索范围内的敌人
            var reachableEnemyUnits = ctx.UnitService.GetAllAliveUnits()
                .Where(u => currentEquipment.Logic.CheckAttackable(u)).ToList();

            // 剔除看不见的敌人
            var visibleCells = ctx.VisionService.CurrentVisibleCells;
            List<Unit.Unit> enemyUnits = reachableEnemyUnits.Where(enemyUnit => visibleCells.Contains(enemyUnit.position)).ToList();
            
            this.Log($"Found {enemyUnits.Count} valid targets for attack.");

            return enemyUnits;
		}

		private void CancelPreview()
		{
			hasConfirmed = false;
			Publish(Context, new RemoveGunLineEvent());
			Publish(Context, PathPreviewEvent.Hide());
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
				Context.EventBus
			);
			
			Context.CommandQueue.EnqueueAndExecute(attackCommand);
            Context?.StateMachine.ChangeState<ExecutingState>();
		}
	}
}
