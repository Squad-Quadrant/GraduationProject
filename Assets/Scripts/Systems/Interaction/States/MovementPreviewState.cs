using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Systems.PathFinding;
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

			this.Log($"Entered - Unit: {ctx.selectedUnit?.name}");

			if (ctx.selectedUnit == null)
			{
				this.LogError("No unit selected! Returning to Idle.");
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
			this.Log("Exited");

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
						this.Log($"Invalid target: {e.CellPosition}");
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
			var pathResult = GetPathFromCache(targetCell);
			var isValid = Context.CachedReachableArea?.CanStopAt(targetCell) ?? false;

			// Update context
			Context.currentPath.Clear();
			if (pathResult.Found)
			{
				Context.currentPath.AddRange(pathResult.Path);
				Context.currentPathCost = pathResult.TotalCost;
			}
			else
				Context.currentPathCost = 0;

			Publish(Context, new PathPreviewEvent(
				pathResult.Found ? pathResult.Path.ToList() : new List<Vector2Int>(),
				pathResult.TotalCost,
				isValid,
				Context.selectedUnit.id
			));
		}

		private void OnActionCancelled(ActionCancelledEvent e) => CancelAndReturn();

		private void CalculateReachableArea(InteractionContext ctx)
		{
			var unit = ctx.selectedUnit;
			var pathfinding = ctx.PathFindingService;

			if (pathfinding == null)
			{
				this.LogError("PathfindingService not available!");
				ctx.validTargetCells.Clear();
				return;
			}

			// Build pathfinding options based on unit capabilities
			// NOTE: Unit.faction is not yet implemented, using null for now
			// which means all other units will block movement
			var options = new PathFindingOptions(
				canPassThroughAllies: true,
				enemiesBlockMovement: true,
				movingUnitFaction: null,  // TODO: add faction to Unit when implemented
				movingUnitId: unit.id,
				canCrossLowWalls: false,  // TODO: could be unit-specific
				canCrossHighWalls: false,
				ignoreTerrainWalkability: false
			);

			// Calculate reachable area (this is where Dijkstra runs)
			// maxMovementPoints is passed separately from options
			var reachableArea = pathfinding.GetReachableArea(
				unit.position,
				unit.stats.moveRange * unit.stats.actionPoints,
				options);

			// Cache the result for path queries during hover
			ctx.CachedReachableArea = reachableArea;

			// Populate validTargetCells with stoppable cells only
			ctx.validTargetCells.Clear();
			ctx.validTargetCells.AddRange(reachableArea.GetStoppableCellsList());

			this.Log($"Calculated reachable area: {reachableArea.StoppableCount} stoppable cells, " +
			         $"{reachableArea.ReachableCount} total reachable");
		}

		private PathResult GetPathFromCache(Vector2Int target)
		{
			var cachedArea = Context.CachedReachableArea;

			if (cachedArea != null) return cachedArea.GetPathTo(target);

			this.LogWarning("No cached reachable area, cannot get path");
			return PathResult.Failure();
		}

		private void CancelAndReturn()
		{
			this.Log("Cancelled, returning to UnitSelected");
			Context.ClearTarget();
			Context.StateMachine.ChangeState<UnitSelectedState>();
		}

		private void ExecuteMove(Vector2Int targetCell)
		{
			this.Log($"Executing move to {targetCell}");

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
	}
}
