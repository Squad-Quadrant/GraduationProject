using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Systems.PathFinding;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class MovementPreviewState : InteractionState
	{
		private Action<CellClickedEvent> _onCellClicked;
		private Action<PointerHoverEvent> _onPointerHover;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;

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

			CalculateReachableArea(ctx);

			this.LogDebug($"Valid target cells: {string.Join(", ", ctx.validTargetCells)}");

			Publish(ctx, new RangeDisplayEvent(
				ERangeType.Movement,
				ctx.validTargetCells,
				ctx.selectedUnit.Position,
				ctx.selectedUnit.id));

			_onCellClicked = OnCellClicked;
			_onPointerHover = OnPointerHover;
			_onBack = OnBack;
			_onEsc = OnEsc;
			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onPointerHover);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Movement));
			Publish(ctx, PathPreviewEvent.Hide());

			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onPointerHover);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);

			_onCellClicked = null;
			_onPointerHover = null;
			_onBack = null;
			_onEsc = null;

			base.OnExit(ctx);
		}

		private void OnCellClicked(CellClickedEvent e)
		{
			if (!Context.CachedReachableArea.CanStopAt(e.CellPosition))
			{
				this.Log($"Invalid target: {e.CellPosition}");
				// todo: Could play error sound or show feedback
				return;
			}

			ExecuteMove(e.CellPosition);
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
			var options = new PathFindingOptions(
				canPassThroughAllies: true,
				enemiesBlockMovement: true,
				movingUnitFaction: unit.stats.faction,
				movingUnitId: unit.id,
				canCrossLowWalls: false,  // TODO: could be unit-specific
				canCrossHighWalls: false,
				ignoreTerrainWalkability: false
			);

			var maxMovementPoints = unit.stats.moveRange * unit.stats.actionPoints;

			var reachableArea = pathfinding.GetReachableArea(
				unit.Position,
				maxMovementPoints,
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

		private void CancelPreview()
		{
			Context.ClearTarget();
			Publish(Context, PathPreviewEvent.Hide());
		}

		private void ExecuteMove(Vector2Int targetCell)
		{
			this.Log($"Executing move to {targetCell}");

			Context.targetCell = targetCell;

			// Create and queue move command
			var moveCommand = new MoveUnitCommand(
				Context.selectedUnit.id,
				Context.selectedUnit.Position,
				targetCell,
				GetPathFromCache(targetCell).Path,
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
