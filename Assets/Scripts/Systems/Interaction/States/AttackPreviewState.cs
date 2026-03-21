using System;
using System.Linq;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Systems.Unit;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class AttackPreviewState : InteractionState
	{
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;

		public AttackPreviewState() : base(InteractionStates.AttackPreview) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			this.Log($"Entered - Unit: {ctx.selectedUnit?.name}");

			if (ctx.selectedUnit == null)
			{
				this.LogError("No unit selected! Returning to Idle.");
				ctx.StateMachine.ChangeState<IdleState>();
				return;
			}

			CalculateReachableTarget(ctx);
            
			Publish(ctx, new RangeDisplayEvent(
				ERangeType.Movement,
				ctx.validTargetCells,
				origin: ctx.selectedUnit.position,
				sourceUnitId: ctx.selectedUnit.id));

			_onUnitClicked = OnUnitClicked;
			_onBack = OnBack;
			_onEsc = OnEsc;
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Movement));
			Publish(ctx, PathPreviewEvent.Hide());

			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);

			_onUnitClicked = null;
			_onBack = null;
			_onEsc = null;

			base.OnExit(ctx);
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (!Context.validTargetCells.Contains(e.CellPosition))
            {
                this.Log($"Clicked cell {e.CellPosition} is not a valid attack target.");
                return;
			}

			ExecuteAttack(e.CellPosition);
		}

		private void OnBack(BackInputEvent e)
		{
			this.Log("Back input → returning to UnitSelected");
			CancelPreview();
			Context.StateMachine.ChangeState<UnitSelectedState>();
		}

		private void OnEsc(EscInputEvent e)
		{
			this.Log("ESC input → resetting to Idle");
			CancelPreview();
			Context.StateMachine.ChangeState<IdleState>();
		}

		private void CalculateReachableTarget(InteractionContext ctx)
		{
			var unit = ctx.selectedUnit;
            int attackRange = unit.attackRange;
            
            // 搜索范围内的敌人
            var enemyUnits = ctx.UnitService.GetUnitsInRange(unit.position, attackRange)
                .Where(u => u.faction == EUnitFaction.Enemy).ToList();
            
            // todo: 计算遮挡并剔除，枪线判断
            
            ctx.validTargetCells.Clear();
            ctx.validTargetCells = enemyUnits.Select(u => u.position).ToList();
            
            this.Log($"Found {enemyUnits.Count} valid targets for attack.");
        }

		private void CancelPreview()
		{
			Context.ClearTarget();
			Publish(Context, PathPreviewEvent.Hide());
		}

		private void ExecuteAttack(Vector2Int target)
		{
            this.Log($"Executing attack on target cell: {target}");

			Context.targetCell = target;
            Context.targetUnit = Context.UnitService.GetUnitAtPosition(target);

			var unit = Context.selectedUnit;
            
			var attackCommand = new UnitAttackCommand(
				unit.id,
                Context.targetUnit.id,
				2, // todo: 计算消耗
				Context.UnitService,
				Context.MapService,
				Context.EventBus
			);

			Context.CommandQueue.EnqueueAndExecute(attackCommand);
            
            // 此处Context有可能为空，是因为在attackCommand的执行链上已经切换了状态并清空了Context，导致此处无法访问到Context。
            if (Context != null)
            {
			    Context.StateMachine.ChangeState<ExecutingState>();
            }
		}
	}
}
