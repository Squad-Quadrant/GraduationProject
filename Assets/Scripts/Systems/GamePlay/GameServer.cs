using System;
using System.Linq;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.Unit;
using Data.Runtime.Events.View;
using Data.Runtime.Events.Vision;
using Presentation.Interaction;
using Systems.Interaction.States;
using Systems.Turn;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Systems.GamePlay
{
	public class GameServer : IGameServer, IDisposable
	{
		public bool IsRunning { get; private set; }
		public bool WaitForPresentation { get; set; } = true;

		private readonly IEventBus _eventBus;
		private readonly ICommandQueue _commandQueue;
		private readonly ITurnService _turnService;
		private readonly IUnitService _unitService;
		private readonly InteractionController _fsm;
		private readonly IVisionService _visionService;

		public GameServer(
			IEventBus eventBus,
			ICommandQueue commandQueue,
			ITurnService turnService,
			IUnitService unitService,
			InteractionController interactionController,
			IVisionService visionService)
		{
			_turnService = turnService;
			_eventBus = eventBus;
			_commandQueue = commandQueue;
			_unitService = unitService;
			_fsm = interactionController;
			_visionService = visionService;

			_eventBus.Subscribe<UnitTurnEndedEvent>(OnUnitTurnEnded);
            _eventBus.Subscribe<UnitDestroyedEvent>(CheckGameOver);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitTurnEndedEvent>(OnUnitTurnEnded);
            _eventBus.Unsubscribe<UnitDestroyedEvent>(CheckGameOver);
			IsRunning = false;
		}

		public void StartGame()
		{
			if (IsRunning)
			{
				this.LogWarning("Game already running!");
				return;
			}

			IsRunning = true;
			this.Log("Starting game...");

			_fsm.StartInteraction();
			StartNewTurn();
		}

        private void CheckGameOver(UnitDestroyedEvent e)
        {
            var aliveUnits = _unitService.GetAllAliveUnits();
            if (aliveUnits.All(u => u.faction == EUnitFaction.Player || u.faction == EUnitFaction.Neutral))
            {
                this.Log("All enemies defeated! You win!");
                _fsm.StateMachine.ChangeState<WaitingForSystemState>();
                return;
            }
            
            if (aliveUnits.All(u => u.faction == EUnitFaction.Enemy || u.faction == EUnitFaction.Neutral))
            {
                this.Log("All player units defeated! You lose!");
                _fsm.StateMachine.ChangeState<WaitingForSystemState>();
                return;
            }
        }

        private void StartNewTurn()
		{
			_turnService.StartTurn();

			this.Log($"Turn {_turnService.TurnNumber}{(WaitForPresentation ? " (awaiting presentation)" : "")} started");

			AwaitThen(AdvanceToNextUnit, cmd => cmd
				.Expect(EPresentationCategory.UI, PresentationType.UI.TurnStart)
			);
		}

		private void AdvanceToNextUnit() // 控制Turn系统推进
		{
			var turnUnit = _turnService.NextUnit();
			if (turnUnit == null)
			{
				this.Log("No more actionable units, ending turn");
				EndCurrentTurn();
				return;
			}

			var unit = _unitService.GetUnit(turnUnit.Id);
			this.Log($"Unit '{unit.id}' turn starting");

			// todo: 这里或许需要根据unit的阵营决定行为
			_eventBus.Publish(new UnitTurnStartedEvent(unit.id, _turnService.TurnNumber));

			var visible = _visionService.CalculateVisibleCells(unit.position, unit.visionRange);
			_eventBus.Publish(new VisionChangedEvent(visible, unit.id));

			AwaitThen(() => StartNewUnitTurn(unit), cmd => cmd
				.Expect(EPresentationCategory.UI, PresentationType.UI.UnitTransition)
			);
		}

		private void StartNewUnitTurn(Systems.Unit.Unit unit) // 实际开始一个新的单位回合，控制状态机推进
		{
			this.Log($"Unit '{unit.id}' is now acting");

			// Control transfers to InteractionFSM (player) or AI system (enemy)
			switch (unit.faction)
			{
				case EUnitFaction.Player: // 直接切换到 UnitSelectedState 让玩家操作
				case EUnitFaction.Enemy:
				case EUnitFaction.Neutral:
				case EUnitFaction.None:
					_fsm.Context.selectedUnit = unit;
					_eventBus.Publish(new UnitSelectedEvent(unit.id, unit.position));
					_fsm.StateMachine.ChangeState<UnitSelectedState>();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnUnitTurnEnded(UnitTurnEndedEvent e)
		{
			this.Log($"Unit '{e.UnitId}' finished acting");

			_fsm.StateMachine.ChangeState<WaitingForSystemState>();

			if (_turnService.IsTurnComplete)
			{
				this.Log("All units have acted, ending turn");
				EndCurrentTurn();
			}
			else
			{
				this.Log("Advancing to next unit");
				AdvanceToNextUnit();
			}
		}

		private void EndCurrentTurn()
		{
			var turnNumber = _turnService.TurnNumber;
			_turnService.EndTurn();

			this.Log($"Turn {turnNumber}{(WaitForPresentation ? " (awaiting presentation)" : "")} ended");

			// TODO: check win/lose conditions here before starting next turn
			// if (CheckGameOver()) return;

			AwaitThen(StartNewTurn, cmd => cmd
				.Expect(EPresentationCategory.UI, PresentationType.UI.TurnEnd)
			);
		}

		private void AwaitThen(Action onComplete, Action<AwaitPresentationCommand> configure)
		{
			if (WaitForPresentation)
			{
				var cmd = new AwaitPresentationCommand(onComplete);
				configure?.Invoke(cmd);
				_commandQueue.EnqueueAndExecute(cmd);
			}
			else
				onComplete();
		}
	}
}
