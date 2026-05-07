using System;
using System.Collections.Generic;
using System.Linq;
using Core.Commands;
using Core.Commands.Events;
using Core.Events;
using Core.Log;
using Presentation.Audio;
using Systems.AI.Actions;
using Systems.AI.Alert;
using Systems.AI.Behavior;
using Systems.AI.Blackboard;
using Systems.AI.Plans;
using Systems.Map;
using Systems.Map.Region;
using Systems.PathFinding;
using Systems.Turn;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Systems.AI
{
	public class AIService : IAIService, IDisposable
	{
		private readonly IEventBus _eventBus;
		private readonly ICommandQueue _commandQueue;
		private readonly IUnitService _unitService;
		private readonly IMapService _mapService;
		private readonly ITurnService _turnService;
		private readonly IPathFindingService _pathFinding;
		private readonly IVisionCalculator _visionCalculator;
		private readonly IAIBlackboardService _blackboardService;
		private readonly IRegionService _regionService;
		private readonly IAlertService _alertService;
		private readonly AudioService _audioService;
		
		private Unit.Unit _currentUnit;
		private Action _onTurnComplete;
		private Action<CommandCompletedEvent> _onCommandCompleted;

		private ITurnPlan _currentPlan;
		private Queue<IAtomicAction> _currentSequence;
        private readonly List<Vector2Int> obscuresCells = new();

		public AIService(
			IEventBus eventBus,
			ICommandQueue commandQueue,
			IUnitService unitService,
			IMapService mapService,
			ITurnService turnService,
			IPathFindingService pathFinding,
			IVisionCalculator visionCalculator,
			IAIBlackboardService blackboardService,
			IRegionService regionService,
			IAlertService alertService,
			AudioService audioService)
		{
			_eventBus = eventBus;
			_commandQueue = commandQueue;
			_unitService = unitService;
			_mapService = mapService;
			_turnService = turnService;
			_pathFinding = pathFinding;
			_visionCalculator = visionCalculator;
			_blackboardService = blackboardService;
			_regionService = regionService;
			_alertService = alertService;
			_audioService = audioService;

			this.Log("Initialized");
		}

		public void Dispose() => CleanupCommandListener();

		public void ExecuteTurn(Unit.Unit unit, Action onComplete)
		{
			_currentUnit = unit;
			_onTurnComplete = onComplete;

			this.Log($"Starting AI turn for '{unit.name}' (AP:{unit.CurrentAp})");
			DecisionLoop();
		}

        public void AddObscuresCells(List<Vector2Int> cells)
        {
            obscuresCells.AddRange(cells);
        }

        public void RemoveObscuresCells(List<Vector2Int> cells)
        {
            obscuresCells.RemoveAll(c => cells.Contains(c));
        }

        public void RemoveAllObscuresCells(List<Vector2Int> cells)
        {
            obscuresCells.Clear();
        }

        private void DecisionLoop()
		{
			if (_currentUnit is not { IsAlive: true } || !_currentUnit.HasAp)
			{
				this.Log("Unit cannot act — ending AI turn");
				EndTurn();
				return;
			}

			if (!_regionService.IsCellUnlocked(_currentUnit.position)) // 未解锁区域的单位不响应
			{
				this.Log($"'{_currentUnit.name}' is in locked region — skipping turn");
				EndTurn();
				return;
			}

			var context = BuildContext(_currentUnit);

			var level = _alertService.GetAlertLevel(_currentUnit.id);
			if (level == EAlertLevel.Calm)
			{
				_currentPlan = null;
				_currentSequence = null;
				ExecuteIdle(context);
				return;
			}

			PlanLoop(context);
		}

		private AIContext BuildContext(Unit.Unit unit) // 构建战场上下文
		{
            var theObscuresCells = new List<Vector2Int>(obscuresCells);
            theObscuresCells.Remove(unit.position);
            
			var visibleCells = _visionCalculator.CalculateVisibleCells(unit.position, unit.visionRange, theObscuresCells);

			if (!unit.CanAIUseEye)
			{
				visibleCells.Clear();
				visibleCells.Add(unit.position);
			}
			
			var enemies = new List<Unit.Unit>();
			var allies = new List<Unit.Unit>();
			foreach (var other in _unitService.GetAllAliveUnits())
			{
				if (other.id == unit.id) continue;
				if (!visibleCells.Contains(other.position)) continue;

				if (unit.IsHostile(other))
					enemies.Add(other);
				else if (other.faction == unit.faction)
					allies.Add(other);
			}

			_blackboardService.ReportVisibleEnemies(unit.faction, _turnService.TurnNumber, unit.id, enemies);

			var options = new PathFindingOptions(
				canPassThroughAllies: true,
				enemiesBlockMovement: true,
				movingUnitFaction: unit.faction,
				movingUnitId: unit.id,
				canCrossLowWalls: false,
				canCrossHighWalls: false,
				ignoreTerrainWalkability: false,
				visibleCells: visibleCells
			);
			int maxMove = unit.moveRange * unit.RemainingMovementAp;
			var reachableArea = _pathFinding.GetReachableArea(unit.position, maxMove, options);

			return new AIContext(
				unit, enemies, allies, reachableArea, visibleCells, _turnService.TurnNumber,
				_eventBus, _unitService, _mapService, _visionCalculator, _blackboardService, options, _pathFinding, _audioService);
		}

		private void ExecuteIdle(AIContext context)
		{
			var unit = _currentUnit;
			var idleTarget = IdleTargetSelector.GetIdleTarget(unit, unit.aiArchetype);

			if (!idleTarget.HasValue) // 已经在合适位置 -→ Wait
			{
				this.Log($"'{unit.id}' idle: at target, waiting");
				EndTurn();
				return;
			}

			var moveTarget = PathFollowingHelper.FindStepTowards(
				unit.position, idleTarget.Value, context.ReachableArea,
				context.PathFinding, context.PathOptions);

			if (moveTarget == unit.position)
			{
				// 走不到任何格 -→ Wait
				this.Log($"'{unit.id}' idle: cannot approach {idleTarget.Value}, waiting");
				EndTurn();
				return;
			}

			var moveAction = new MoveAction(moveTarget);
			ExecuteAction(moveAction, context);
		}

		private void PlanLoop(AIContext context)
		{
			int retry = 0;
			while (true)
			{
				if (retry > 3)
				{
					this.LogError("exceeded the maximum number of attempts, ending turn");
					EndTurn();
					return;
				}

				if (_currentPlan == null) // 没 plan → 选一个
				{
					if (!TrySelectNewPlan(context, out _currentPlan))
					{
						this.LogWarning("No viable plan, ending turn");
						EndTurn();
						return;
					}
					_currentSequence = _currentPlan.BuildActionSequence(context);
				}

				if (_currentPlan.ShouldAbort(context))
				{
					this.Log($"Plan '{_currentPlan.Name}' aborted, replanning");
					_currentPlan = null;
					_currentSequence = null;
					retry += 1;
					continue;
				}

				if (_currentSequence == null || _currentSequence.Count == 0)
				{
					this.Log($"Plan '{_currentPlan.Name}' completed");
					_currentPlan = null;
					_currentSequence = null;
					EndTurn();
					return;
				}

				var action = _currentSequence.Dequeue();
				ExecuteAction(action, context);
				return;
			}
		}

		private bool TrySelectNewPlan(AIContext context, out ITurnPlan newPlan)
		{
			var candidates = GenerateCandidatePlans(context);

			ITurnPlan best = null;
			float bestScore = float.NegativeInfinity;
			foreach (var plan in candidates)
			{
				if (!plan.IsViable(context)) continue;
				float score = plan.Score(context);
				if (score <= bestScore) continue;
				bestScore = score;
				best = plan;
			}

			if (best == null)
			{
				newPlan = null;
				return false;
			}

			newPlan = best;

			this.Log($"Selected plan: {best.Name} (score: {bestScore:F2})");
			return true;
		}

		private static List<ITurnPlan> GenerateCandidatePlans(AIContext context)
		{
			var plans = new List<ITurnPlan>();

			plans.AddRange(context.Enemies.Select(enemy => new EngagePlan(enemy)));

			var board = context.BlackboardService.GetBlackboard(context.Self.faction);
			if (board != null)
				plans.AddRange(board.KnownEnemies.Values.Select(known => new SearchPlan(known)));

			plans.Add(new ReloadPlan());
			plans.Add(new WaitPlan());

			return plans;
		}

		private void ExecuteAction(IAtomicAction action, AIContext context)
		{
			var cmd = action.CreateCommand(context);
			if (cmd == null)
			{
				this.LogWarning($"Failed to create command for {action} — aborting plan");
				AbortAndReplan();
				return;
			}
			this.Log($"Executing: {action}");
			ExecuteCommandThenContinue(cmd);
		}

		private void AbortAndReplan()
		{
			_currentPlan = null;
			_currentSequence = null;
			DecisionLoop();
		}

		private void ExecuteCommandThenContinue(ICommand command)
		{
			_onCommandCompleted = OnCommandCompleted;
			_eventBus.Subscribe(_onCommandCompleted);

			_commandQueue.EnqueueAndExecute(command);
		}

		private void OnCommandCompleted(CommandCompletedEvent e)
		{
			CleanupCommandListener();
			DecisionLoop();
		}

		private void EndTurn()
		{
			this.Log($"AI turn complete for '{_currentUnit?.name}'");

			CleanupCommandListener();
			var callback = _onTurnComplete;
			_currentUnit = null;
			_onTurnComplete = null;
			_currentPlan = null;
			_currentSequence = null;
			callback?.Invoke();
		}

		private void CleanupCommandListener()
		{
			if (_onCommandCompleted == null) return;
			_eventBus.Unsubscribe(_onCommandCompleted);
			_onCommandCompleted = null;
		}
	}
}
