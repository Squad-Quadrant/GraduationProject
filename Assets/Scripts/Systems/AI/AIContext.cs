using System.Collections.Generic;
using Core.Events;
using Data.Runtime;
using Presentation.Audio;
using Systems.AI.Actions;
using Systems.AI.Blackboard;
using Systems.AI.Config;
using Systems.Map;
using Systems.PathFinding;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Systems.AI
{
	public class AIContext
	{
		public Unit.Unit Self { get; }
		public AIArchetype Archetype { get; }
		public List<Unit.Unit> Enemies { get; }
		public List<Unit.Unit> Allies { get; }
		public ReachableAreaResult ReachableArea { get; }
		public HashSet<Vector2Int> VisibleCells { get; }
		public int CurrentTurn { get; }

		public IEventBus EventBus { get; }
		public IUnitService UnitService { get; }
		public IMapService MapService { get; }
		public IVisionCalculator VisionCalculator { get; }
		public IVisionService VisionService { get; }
		public IAIBlackboardService BlackboardService { get; }
		public IPathFindingService PathFinding { get; }
		public AudioService AudioService { get; }

		public PathFindingOptions PathOptions { get; }

		public AIContext(
			Unit.Unit self,
			List<Unit.Unit> enemies,
			List<Unit.Unit> allies,
			ReachableAreaResult reachableArea,
			HashSet<Vector2Int> visibleCells,
			int currentTurn,
			IEventBus eventBus,
			IUnitService unitService,
			IMapService mapService,
			IVisionCalculator visionCalculator,
			IVisionService visionService,
			IAIBlackboardService blackboardService,
			PathFindingOptions pathOptions,
			IPathFindingService pathFinding,
			AudioService audioService)
		{
			Self = self;
			Archetype = self.aiArchetype;
			Enemies = enemies;
			Allies = allies;
			ReachableArea = reachableArea;
			VisibleCells = visibleCells;
			CurrentTurn = currentTurn;

			EventBus = eventBus;
			UnitService = unitService;
			MapService = mapService;
			VisionCalculator = visionCalculator;
			VisionService = visionService;
			BlackboardService = blackboardService;
			PathOptions = pathOptions;
			PathFinding = pathFinding;
			AudioService = audioService;
		}

		public bool TryEnqueueAction(IAtomicAction action, ref Queue<IAtomicAction> queue)
		{
			if (action.ActionType == EActionType.AI)
			{
				queue.Enqueue(action);
				return true;
			}

			var availableActions = Self.GetAvailableActions();
			if (!availableActions.Exists(ability => ability.ActionType == action.ActionType && ability.IsAvailable))
				return false;
			queue.Enqueue(action);
			return true;
		}
	}
}
