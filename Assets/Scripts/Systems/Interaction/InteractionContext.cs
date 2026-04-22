using System;
using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Core.FSM;
using Data.Runtime;
using Systems.AreaEffect;
using Systems.Damage;
using Systems.Interaction.Targeting;
using Systems.Map;
using Systems.Map.Region;
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
        public IAreaEffectService AreaEffectService { get; }

        public IRegionService RegionService { get; }

		#endregion

		public StateMachine<InteractionContext> StateMachine { get; internal set; }

		public Unit.Unit selectedUnit; // 现在的selectedUnit在绝大多数情况下都不应该为空

		public EActionType currentAction;

		public MovementSimulationResult LastSimulationResult;

		public ITargeted PendingTarget;

		public InteractionContext(
			IEventBus eventBus,
			IUnitService unitService,
			IMapService mapService,
			ITurnService turnService,
			ICommandQueue commandQueue,
			IPathFindingService pathFindingService,
            IVisionService visionService,
			IVisionCalculator visionCalculator,
			IDamageService damageService,
			IAreaEffectService areaEffectService,
			IRegionService regionService)
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
            AreaEffectService = areaEffectService;
            RegionService = regionService;
		}

		public void ClearSelection()
		{
			selectedUnit = null;
			PendingTarget = null;
		}

		public Unit.Unit GetCurrentTurnUnit()
		{
			var turnUnit = TurnService.ActiveUnit;
			if (turnUnit == null) return null;
			return UnitService.TryGetUnit(turnUnit.Id, out var unit) ? unit : null;
		}

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
