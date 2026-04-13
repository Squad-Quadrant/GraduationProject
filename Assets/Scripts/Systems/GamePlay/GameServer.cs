using System;
using System.Linq;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.Unit;
using Data.Runtime.Events.View;
using Presentation.Interaction;
using Systems.AI;
using Systems.Interaction.States;
using Systems.Turn;
using Systems.Unit;
using Systems.Vision;

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

		private readonly PlayerTurnController _playerController;
		private readonly AITurnController _aiController;

		public GameServer(
			IEventBus eventBus,
			ICommandQueue commandQueue,
			ITurnService turnService,
			IUnitService unitService,
			InteractionController interactionController,
			IVisionService visionService,
			IAIService aiService)
		{
			_turnService = turnService;
			_eventBus = eventBus;
			_commandQueue = commandQueue;
			_unitService = unitService;
			_fsm = interactionController;
			_visionService = visionService;

			_playerController = new PlayerTurnController(_eventBus, _fsm);
			_aiController = new AITurnController(_turnService, aiService);

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
			_visionService.RecalculateSharedVision();
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
            
            if (aliveUnits.All(u => u.faction is EUnitFaction.Enemy or EUnitFaction.Neutral))
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
			bool isPlayer = unit.faction == EUnitFaction.Player;

			if (!isPlayer) _visionService.ClearSpottedMark(unit.id);

			bool visibleToPlayer = isPlayer || _visionService.IsCellVisible(unit.position);
			_eventBus.Publish(new UnitTurnStartedEvent(unit.id, _turnService.TurnNumber, visibleToPlayer));

			this.Log($"Unit '{unit.id}' ({unit.faction}) turn starting{(visibleToPlayer ? "" : " [hidden from player]")}");

			AwaitThen(() => StartNewUnitTurn(unit), cmd => cmd
				.Expect(EPresentationCategory.UI, PresentationType.UI.UnitTransition)
			);
		}

		private void StartNewUnitTurn(Systems.Unit.Unit unit) // 实际开始一个新的单位回合
		{
			this.Log($"Unit '{unit.id}' is now acting");
			ResolveTurnController(unit).BeginTurn(unit);
		}

		private ITurnController ResolveTurnController(Unit.Unit unit) => unit.faction switch
		{
			EUnitFaction.Player => _playerController,
			_ => _aiController
		};

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
