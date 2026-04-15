using System;
using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Core.FSM;
using Data.Runtime;
using Systems.Damage;
using Systems.Map;
using Systems.PathFinding;
using Systems.PathFinding.MovementSimulation;
using Systems.Turn;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Systems.Interaction
{
	[Serializable]
	public class InteractionContext
	{
		#region Services

		public IEventBus EventBus { get; }
		public IUnitService UnitService { get; }
		public IMapService MapService { get; }
		public ITurnService TurnService { get; }
		public ICommandQueue CommandQueue { get; }
		public IPathFindingService PathFindingService { get; }
        public IVisionService VisionService { get; }
        public IVisionCalculator VisionCalculator { get; }
        public IDamageService DamageService { get; }

		#endregion

		public StateMachine<InteractionContext> StateMachine { get; internal set; }

		public Unit.Unit selectedUnit;

		public EActionType currentAction;

		public MovementSimulationResult LastSimulationResult;

		public InteractionContext(
			IEventBus eventBus,
			IUnitService unitService,
			IMapService mapService,
			ITurnService turnService,
			ICommandQueue commandQueue,
			IPathFindingService pathFindingService,
            IVisionService visionService,
			IVisionCalculator visionCalculator,
			IDamageService damageService)
		{
			EventBus = eventBus;
			UnitService = unitService;
			MapService = mapService;
			TurnService = turnService;
			CommandQueue = commandQueue;
			PathFindingService = pathFindingService;
            VisionService = visionService;
            VisionCalculator = visionCalculator;
            DamageService = damageService;
		}

		public void ClearSelection()
		{
			selectedUnit = null;
		}

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
