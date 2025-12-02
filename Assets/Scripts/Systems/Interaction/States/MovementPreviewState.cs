using System;
using System.Collections.Generic;
using Data.Runtime.Commands;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class MovementPreviewState : InteractionState
	{
		private Action<CellClickedEvent> _onCellClicked;
		private Action<PointerHoverEvent> _onPointerHover;
		private Action<ActionCancelledEvent> _onActionCancelled;

		public MovementPreviewState() : base(InteractionStates.MovementPreview) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			Log($"[MovementPreviewState] Entered - Unit: {ctx.selectedUnit?.name}");

			if (ctx.selectedUnit == null)
			{
				LogError("[MovementPreviewState] No unit selected! Returning to Idle.");
				ctx.StateMachine.ChangeState<IdleState>();
				return;
			}

			// calculate movement preview path here

			Publish(ctx, new RangeDisplayEvent(
				ERangeType.Movement,
				ctx.validTargetCells,
				ctx.selectedUnit.position,
				ctx.selectedUnit.id));

			_onCellClicked = OnCellClicked;
			_onPointerHover = OnPointerHover;
			_onActionCancelled = OnActionCancelled;

			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onPointerHover);
			Subscribe(ctx, _onActionCancelled);
		}

		public override void OnExit(InteractionContext ctx)
		{
			Log("[MovementPreviewState] Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Movement));
			Publish(ctx, PathPreviewEvent.Hide());

			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onPointerHover);
			Unsubscribe(ctx, _onActionCancelled);

			_onCellClicked = null;
			_onPointerHover = null;
			_onActionCancelled = null;

			base.OnExit(ctx);
		}

		private void OnCellClicked(CellClickedEvent e)
		{
			switch (e.MouseButton)
			{
				case 0: // Left-click - check if valid target
					if (!Context.validTargetCells.Contains(e.CellPosition))
					{
						Log($"[MovementPreviewState] Invalid target: {e.CellPosition}");
						// todo: Could play error sound or show feedback
						return;
					}
					ExecuteMove(e.CellPosition);
					break;

				case 1: // Right-click = cancel
					CancelAndReturn();
					break;
			}
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			if (!e.CellPosition.HasValue)
			{
				// Pointer outside map - hide path
				Publish(Context, PathPreviewEvent.Hide());
				Context.currentPath.Clear();
				return;
			}

			var targetCell = e.CellPosition.Value;

			// Calculate path to hovered cell
			var path = CalculatePath(Context.selectedUnit.position, targetCell);
			var isValid = Context.validTargetCells.Contains(targetCell);
			var cost = CalculatePathCost(path);

			// Update context
			Context.currentPath.Clear();
			Context.currentPath.AddRange(path);
			Context.currentPathCost = cost;

			Publish(Context, new PathPreviewEvent(
				path,
				cost,
				isValid,
				Context.selectedUnit.id
			));
		}

		private void OnActionCancelled(ActionCancelledEvent e) => CancelAndReturn();

		private void CancelAndReturn()
		{
			Log("[MovementPreviewState] Cancelled, returning to UnitSelected");
			Context.ClearTarget();
			Context.StateMachine.ChangeState<UnitSelectedState>();
		}

		private void ExecuteMove(Vector2Int targetCell)
		{
			Log($"[MovementPreviewState] Executing move to {targetCell}");

			Context.targetCell = targetCell;

			// Create and queue move command
			var moveCommand = new MoveUnitCommand(
				Context.selectedUnit.id,
				Context.selectedUnit.position,
				targetCell,
				new List<Vector2Int>(Context.currentPath),
				Context.UnitService,
				Context.MapService,
				Context.EventBus
			);

			Context.CommandQueue.EnqueueAndExecute(moveCommand);

			// Transition to executing state
			Context.StateMachine.ChangeState<ExecutingState>();
		}

		private IReadOnlyList<Vector2Int> CalculatePath(Vector2Int selectedUnitPosition, Vector2Int targetCell)
		{
			return new List<Vector2Int>(); // Placeholder implementation
		}

		private int CalculatePathCost(IReadOnlyList<Vector2Int> path)
		{
			return 1; // Placeholder implementation
		}
	}
}
