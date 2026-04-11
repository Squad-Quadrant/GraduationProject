using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Systems.PathFinding;
using Systems.PathFinding.MovementSimulation;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class MovementPreviewState : InteractionState
	{
		private Action<CellClickedEvent> _onCellClicked;
		private Action<PointerHoverEvent> _onPointerHover;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;

		private ReachableAreaResult _reachableArea;

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

			_reachableArea = CalculateReachableArea(ctx.selectedUnit, ctx.PathFindingService, ctx.VisibleCells);
			var stoppableCells = _reachableArea.GetStoppableCellsList();
			var apMap = _reachableArea.CostMap
				.ToDictionary(
					cellCost => cellCost.Key,
					cellCost => ctx.selectedUnit.CalculateMovementApCost(cellCost.Value));

			this.LogDebug($"Valid target cells: {string.Join(", ", stoppableCells)}");

			Publish(ctx, new RangeDisplayEvent(
				ERangeType.Movement,
				stoppableCells,
				apMap,
				ctx.selectedUnit.position,
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
			Publish(ctx, CursorInfoEvent.Hide());

			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onPointerHover);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);

			_onCellClicked = null;
			_onPointerHover = null;
			_onBack = null;
			_onEsc = null;
			_reachableArea = null;

			base.OnExit(ctx);
		}

		private void OnCellClicked(CellClickedEvent e)
		{
			if (!_reachableArea.CanStopAt(e.CellPosition))
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
				Publish(Context, PathPreviewEvent.Hide());
				Publish(Context, CursorInfoEvent.Hide());
				return;
			}

			var targetCell = e.CellPosition.Value;
			var pathResult = GetPath(targetCell);
			var isValid = _reachableArea?.CanStopAt(targetCell) ?? false;

			Publish(Context, new PathPreviewEvent(
				pathResult.Found ? pathResult.Path.ToList() : new List<Vector2Int>(),
				pathResult.TotalCost,
				isValid,
				Context.selectedUnit.id
			));

			var terrainName = Context.MapService.Data.GetCell(targetCell)?.Terrain.ToString() ?? "";
			if (pathResult.Found)
			{
				var unit = Context.selectedUnit;
				int apCost = unit.CalculateMovementApCost(pathResult.TotalCost);

				Publish(Context, CursorInfoEvent.ForMovement(
					targetCell, e.WorldPosition, terrainName,
					apCost, unit.CurrentAp - apCost, isValid));
			}
			else
				Publish(Context, CursorInfoEvent.ForTerrain(targetCell, e.WorldPosition, terrainName));
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

		private ReachableAreaResult CalculateReachableArea(Unit.Unit selectedUnit, IPathFindingService pathfinding, HashSet<Vector2Int> visibleCells)
		{
			var options = new PathFindingOptions(
				canPassThroughAllies: true,
				enemiesBlockMovement: true,
				movingUnitFaction: selectedUnit.faction,
				movingUnitId: selectedUnit.id,
				canCrossLowWalls: false,  // TODO: could be unit-specific
				canCrossHighWalls: false,
				ignoreTerrainWalkability: false,
				visibleCells: visibleCells
			);

			var maxMovementPoints = selectedUnit.moveRange * selectedUnit.RemainingMovementAp;

			var reachableArea = pathfinding.GetReachableArea(
				selectedUnit.position,
				maxMovementPoints,
				options);

			this.Log($"Calculated reachable area: {reachableArea.StoppableCount} stoppable cells, " +
			         $"{reachableArea.ReachableCount} total reachable" +
			         $"(vision: {visibleCells.Count} cells)");

			return reachableArea;
		}

		private PathResult GetPath(Vector2Int target)
		{
			if (_reachableArea != null)
				return _reachableArea.GetPathTo(target);

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

			var fullPathResult = GetPath(targetCell);
			if (!fullPathResult.Found)
			{
				this.LogError($"No path found to {targetCell}");
				return;
			}

			var unit = Context.selectedUnit;
			var simResult = MovementSimulator.Simulate(
				fullPathResult.Path,
				unit,
				Context.VisibleCells,
				Context.VisionService,
				Context.UnitService);

			Context.LastSimulationResult = simResult;

			this.Log($"Movement interrupted: {simResult.WasInterrupted}");

			var actualPath = simResult.ActualPath;
			var actualDestination = actualPath[^1];

			this.Log($"{unit.position} - {actualDestination}");

			if (actualDestination == unit.position)
			{
				this.Log("Truncated to origin — no movement needed");
				unit.CurrentAp -= 1;
				Context.StateMachine.ChangeState<ExecutingState>();
				return;
			}

			int pathCost = _reachableArea.GetCostTo(actualDestination);
			int apCost = unit.CalculateMovementApCost(pathCost);

			// Create and queue move command
			var moveCommand = new MoveUnitCommand(
				unit.id,
				unit.position,
				actualDestination,
				actualPath,
				apCost,
				Context.UnitService,
				Context.MapService,
				Context.EventBus
			);

			Context.CommandQueue.EnqueueAndExecute(moveCommand);
			Context.StateMachine.ChangeState<ExecutingState>();
		}
	}
}
