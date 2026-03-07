using System;
using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Core.FSM;
using Data.Runtime;
using Systems.Map;
using Systems.PathFinding;
using Systems.Turn;
using Systems.Unit;
using UnityEngine;

namespace Systems.Interaction
{
	[Serializable]
	public class InteractionContext
	{
		public static readonly Vector2Int InvalidCell = new (-1, -1);

		#region Services

		public IEventBus EventBus { get; }
		public IUnitService UnitService { get; }
		public IMapService MapService { get; }
		public ITurnService TurnService { get; }
		public ICommandQueue CommandQueue { get; }

		public IPathFindingService PathFindingService { get; }

		#endregion

		public StateMachine<InteractionContext> StateMachine { get; internal set; }

		public Unit.Unit selectedUnit;

		public Unit.Unit targetUnit;

		public Vector2Int targetCell = InvalidCell;

		public EActionType currentAction;

		public List<Vector2Int> validTargetCells = new();
        
        // public List<Unit.Unit> validTargetUnits = new();

		public List<Vector2Int> currentPath = new();

		public ReachableAreaResult CachedReachableArea;

		public InteractionContext(
			IEventBus eventBus,
			IUnitService unitService,
			IMapService mapService,
			ITurnService turnService,
			ICommandQueue commandQueue,
			IPathFindingService pathFindingService)
		{
			EventBus = eventBus;
			UnitService = unitService;
			MapService = mapService;
			TurnService = turnService;
			CommandQueue = commandQueue;
			PathFindingService = pathFindingService;
		}

		/// <summary>
		/// Clears all selection-related state.
		/// Call this when deselecting a unit or returning to idle.
		/// </summary>
		public void ClearSelection()
		{
			selectedUnit = null;
			targetCell = InvalidCell;
			targetUnit = null;
			validTargetCells.Clear();
			currentPath.Clear();
		}

		/// <summary>
		/// Clears only the target-related state.
		/// Call this when cancelling target selection but keeping unit selected.
		/// </summary>
		public void ClearTarget()
		{
			targetCell = InvalidCell;
			targetUnit = null;
			currentPath.Clear();
		}

		public bool HasTarget => targetCell != InvalidCell || targetUnit != null;

		/// <summary>
		/// Gets the currently acting unit from TurnService.
		/// Returns null if no unit is currently acting.
		/// </summary>
		public Unit.Unit GetCurrentTurnUnit()
		{
			var turnUnit = TurnService.ActiveUnit;
			if (turnUnit == null) return null;
			return UnitService.TryGetUnit(turnUnit.Id, out var unit) ? unit : null;
		}

		/// <summary>
		/// Checks if the given unit belongs to the current player and can act.
		/// </summary>
		public bool CanControlUnit(Unit.Unit unit)
		{
			if (unit == null) return false;

			var currentTurnUnit = TurnService.ActiveUnit;
			if (currentTurnUnit == null || currentTurnUnit.Id != unit.id)
				return false;

			return currentTurnUnit.CanAct;
		}
	}
}
