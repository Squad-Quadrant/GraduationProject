using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class TargetingState : InteractionState
	{
		private Action<CellClickedEvent> _onCellClicked;
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<PointerHoverEvent> _onPointerHover;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;

		private IReadOnlyList<Vector2Int> _validCells;
		private Vector2Int? _lastAoEHoverCell;

		public TargetingState() : base(InteractionStates.Targeting) { }

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

			if (ctx.PendingTargeting == null)
			{
				this.LogError("pendingTargeting is null! Expected ItemSelectionState to set this before transitioning. Returning to UnitSelected.");
				ctx.StateMachine.ChangeState<UnitSelectedState>();
				return;
			}

			_validCells = ctx.PendingTargeting.GetValidCells(ctx) ?? Array.Empty<Vector2Int>();
			Publish(ctx, new RangeDisplayEvent(
				ERangeType.Interact,
				_validCells,
				origin: ctx.selectedUnit.position,
				sourceUnitId: ctx.selectedUnit.id));

			_onCellClicked = OnCellClicked;
			_onUnitClicked = OnUnitClicked;
			_onPointerHover = OnPointerHover;
			_onBack = OnBack;
			_onEsc = OnEsc;

			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onPointerHover);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Interact));
			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
			Publish(ctx, CursorInfoEvent.Hide());

			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onPointerHover);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);

			_onCellClicked = null;
			_onUnitClicked = null;
			_onPointerHover = null;
			_onBack = null;
			_onEsc = null;
			_validCells = null;
			_lastAoEHoverCell = null;

			ctx.PendingTargeting = null;

			base.OnExit(ctx);
		}

		private void OnCellClicked(CellClickedEvent e) => ConfirmSelection(e.CellPosition);

		private void OnUnitClicked(UnitClickedEvent e) => ConfirmSelection(e.CellPosition);

		private void ConfirmSelection(Vector2Int cellPosition)
		{
			if (!_validCells.Contains(cellPosition))
			{
				this.Log($"Clicked cell {cellPosition} is not in valid target set. Ignoring.");
				return;
			}

			var targeted = Context.PendingTargeting;
			if (!targeted.ValidateTarget(cellPosition, Context))
			{
				this.Log($"Cell {cellPosition} failed ValidateTarget. Ignoring.");
				// todo: UI 反馈玩家（命中率 0 / 落点无效等具体原因，M5 补）
				return;
			}

			this.Log($"Executing targeting at cell {cellPosition}");
			var cmd = targeted.CreateCommand(cellPosition, Context);
			Context.CommandQueue.EnqueueAndExecute(cmd);
			StateMachine(Context).ChangeState<ExecutingState>();
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			PublishBasicCursorInfo(Context, e);

			if (!e.CellPosition.HasValue || !_validCells.Contains(e.CellPosition.Value))
			{
				Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
				_lastAoEHoverCell = null;
				return;
			}

			var hoverCell = e.CellPosition.Value;
			if (_lastAoEHoverCell == hoverCell) return;    // 同格不重复发
			_lastAoEHoverCell = hoverCell;

			var aoeCells = Context.PendingTargeting.GetAreaEffectPreview(hoverCell);
			if (aoeCells == null || aoeCells.Count == 0)
			{
				Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
				return;
			}

			Publish(Context, new RangeDisplayEvent(ERangeType.AreaEffectPreview, aoeCells, origin: hoverCell));
		}

		private void OnBack(BackInputEvent e)
		{
			this.Log("Back → ItemSelection (reselect item)");
			StateMachine(Context).ChangeState<ItemSelectionState>();
		}

		private void OnEsc(EscInputEvent e)
		{
			this.Log("Esc → Idle");
			StateMachine(Context).ChangeState<IdleState>();
		}
	}
}
