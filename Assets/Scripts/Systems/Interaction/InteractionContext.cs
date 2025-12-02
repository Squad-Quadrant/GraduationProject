using System;
using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Core.FSM;
using Data.Runtime;
using Systems.Interaction.States;
using Systems.Map;
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

		#endregion

		public StateMachine<InteractionContext> StateMachine { get; internal set; }

		public Unit.Unit selectedUnit;

		public Unit.Unit targetUnit;

		public Vector2Int targetCell = InvalidCell;

		public EActionType currentAction;

		public List<EActionType> availableActions = new();

		public List<Vector2Int> validTargetCells = new();

		public List<Vector2Int> currentPath = new();

		public int currentPathCost;

		public InteractionContext(
			IEventBus eventBus,
			IUnitService unitService,
			IMapService mapService,
			ITurnService turnService,
			ICommandQueue commandQueue)
		{
			EventBus = eventBus;
			UnitService = unitService;
			MapService = mapService;
			TurnService = turnService;
			CommandQueue = commandQueue;
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
			currentAction = EActionType.None;
			availableActions.Clear();
			validTargetCells.Clear();
			currentPath.Clear();
			currentPathCost = 0;
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
			currentPathCost = 0;
		}

		public bool HasTarget => targetCell != InvalidCell || targetUnit != null;

		/// <summary>
		/// Gets the currently acting unit from TurnService.
		/// Returns null if no unit is currently acting.
		/// </summary>
		public Unit.Unit GetCurrentTurnUnit()
		{
			var turnUnit = TurnService.GetCurrentUnit();
			if (turnUnit == null) return null;

			return UnitService.TryGetUnit(turnUnit.Id, out var unit) ? unit : null;
		}

		/// <summary>
		/// Checks if the given unit belongs to the current player and can act.
		/// </summary>
		public bool CanControlUnit(Unit.Unit unit)
		{
			if (unit == null) return false;

			// Check if it's this unit's turn
			var currentTurnUnit = TurnService.GetCurrentUnit();
			if (currentTurnUnit == null || currentTurnUnit.Id != unit.id)
				return false;

			// todo: may dont need to check CanAct
			// Check if unit can act
			return currentTurnUnit.CanAct;
		}
	}
}
