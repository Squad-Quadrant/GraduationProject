using System;
using System.Collections.Generic;
using System.Linq;
using Core.Commands;
using Core.Commands.Events;
using Core.Events;
using Core.Log;
using Data.Runtime.Commands;
using Systems.AI.Evaluation;
using Systems.Map;
using Systems.PathFinding;
using Systems.Turn;
using Systems.Unit;
using Systems.Vision;

namespace Systems.AI
{
	public class AIService : IAIService
	{
		private readonly IEventBus _eventBus;
		private readonly ICommandQueue _commandQueue;
		private readonly IUnitService _unitService;
		private readonly IMapService _mapService;
		private readonly ITurnService _turnService;
		private readonly IPathFindingService _pathFinding;
		private readonly IVisionService _visionService;

		private readonly List<IActionEvaluator> _evaluators;

		private Unit.Unit _currentUnit;
		private Action _onTurnComplete;
		private Action<CommandCompletedEvent> _onCommandCompleted;

		public AIService(
			IEventBus eventBus,
			ICommandQueue commandQueue,
			IUnitService unitService,
			IMapService mapService,
			ITurnService turnService,
			IPathFindingService pathFinding,
			IVisionService visionService)
		{
			_eventBus = eventBus;
			_commandQueue = commandQueue;
			_unitService = unitService;
			_mapService = mapService;
			_turnService = turnService;
			_pathFinding = pathFinding;
			_visionService = visionService;

			// 注册评估器
			_evaluators = new List<IActionEvaluator>
			{
				new WaitEvaluator(),
				new MoveEvaluator(),
                new AttackEvaluator(),
                new ReloadEvaluator()
			};

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

		private void DecisionLoop()
		{
			if (_currentUnit == null || !_currentUnit.IsAlive || !_currentUnit.HasAp)
			{
				this.Log("Unit cannot act — ending AI turn");
				EndTurn();
				return;
			}

			var context = BuildContext(_currentUnit);
			var best = EvaluateAndSelect(context);

			this.Log($"Decision: {best}");

			switch (best.ActionType)
			{
				case EAIActionType.Wait:
					EndTurn();
					break;

				case EAIActionType.Move:
					ExecuteMove(best, context);
					break;

				case EAIActionType.Attack:
					ExecuteAttack(best);
					break;
                
                case EAIActionType.Reload:
                    ExecuteReload(best);
                    break;

				default:
					this.LogWarning($"Unhandled action type: {best.ActionType}");
					EndTurn();
					break;
			}
		}

		private AIContext BuildContext(Unit.Unit unit) // 构建战场上下文
		{
			var visibleCells = _visionService.CalculateVisibleCells(unit.position, unit.visionRange);

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
			int maxMove = unit.moveRange * unit.CurrentAp;
			var reachableArea = _pathFinding.GetReachableArea(unit.position, maxMove, options);

			return new AIContext(unit, enemies, allies, reachableArea, visibleCells);
		}

		private AIActionOption EvaluateAndSelect(AIContext context) // 跑一遍所有的评估器，选出当下最好的
		{
			var allOptions = new List<AIActionOption>();

			foreach (var evaluator in _evaluators)
			{
				var options = evaluator.Evaluate(context);
				if (options != null)
					allOptions.AddRange(options);
			}

			return allOptions.OrderByDescending(o => o.Score).First();
		}

		private void ExecuteMove(AIActionOption option, AIContext context)
		{
			var target = option.MoveTarget!.Value;
			var pathResult = context.ReachableArea.GetPathTo(target);

			if (!pathResult.Found)
			{
				this.LogWarning($"No path to {target} — falling back to Wait");
				EndTurn();
				return;
			}

			var unit = _currentUnit;
			int apCost = unit.CalculateMovementApCost(pathResult.TotalCost);

			var cmd = new MoveUnitCommand(
				unit.id,
				unit.position,
				target,
				pathResult.Path,
				apCost,
				_unitService,
				_mapService,
				_eventBus
			);

			ExecuteCommandThenContinue(cmd);
		}

		private void ExecuteAttack(AIActionOption option)
		{
			var unit = _currentUnit;

			var cmd = new UnitAttackCommand(
				unit.id,
				option.TargetUnitId,
				1,
				option.EquipmentAction,
				_unitService,
				_mapService,
				_eventBus
			);

			ExecuteCommandThenContinue(cmd);
		}
        
        private void ExecuteReload(AIActionOption option)
        {
            var unit = _currentUnit;
            
            var cmd = new UnitReloadCommand(
                unit,
                1,
                _eventBus
            );

            ExecuteCommandThenContinue(cmd);
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
