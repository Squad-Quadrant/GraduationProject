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

			_playerController = new PlayerTurnController(_eventBus, _turnService, _fsm);
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
			this.Log($"Turn {_turnService.TurnNumber}{(WaitForPresentation ? " (awaiting presentation)" : "")} started");

			AwaitThen(
				() => _turnService.StartTurn(),
				AdvanceToNextUnit,
				cmd => cmd
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

			bool visibleToPlayer;
			var faction = turnUnit.Faction;

			switch (faction)
			{
				case EUnitFaction.Player:
					visibleToPlayer = true;
					break;

				case EUnitFaction.Enemy:
				case EUnitFaction.Neutral:
					_visionService.ClearSpottedMark(turnUnit.Id);
					visibleToPlayer = _visionService.IsCellVisible(turnUnit.CellPosition);
					break;

				case EUnitFaction.None:
				default:
					this.LogWarning($"Unexpected Faction '{faction}' for turnUnit '{turnUnit.Id}', treating as not visible");
					visibleToPlayer = false;
					break;
			}

			AwaitThen(
				() => _eventBus.Publish(new UnitTurnStartedEvent(
					turnUnit.Id,
					turnUnit.DisplayName,
					_turnService.TurnNumber,
					visibleToPlayer,
					turnUnit.CellPosition)),
				() => StartNewUnitTurn(turnUnit),
				cmd => cmd
					.Expect(EPresentationCategory.Camera, PresentationType.Camera.Focus)
					.Expect(EPresentationCategory.UI, PresentationType.UI.UnitTransition)
			);

			this.Log($"TurnUnit '{turnUnit.DisplayName}'({turnUnit.Id}, {turnUnit.Faction}) turn starting{(visibleToPlayer ? "" : " [hidden from player]")}");
		}

		private void StartNewUnitTurn(ITurnUnit turnUnit) // 实际开始一个新的单位回合
		{
			this.Log($"Unit '{turnUnit.Id}' is now acting");
			ResolveTurnController(turnUnit).BeginTurn(turnUnit);
		}

		private ITurnController ResolveTurnController(ITurnUnit turnUnit) => turnUnit.Faction switch
		{
			EUnitFaction.Player => _playerController,
			EUnitFaction.Enemy => _aiController,
			EUnitFaction.Neutral => throw new ArgumentOutOfRangeException(nameof(turnUnit), turnUnit.Faction, "Unsupported faction"),
			EUnitFaction.None => throw new ArgumentOutOfRangeException(nameof(turnUnit), turnUnit.Faction, "Unsupported faction"),
			_ => throw new ArgumentOutOfRangeException(nameof(turnUnit), turnUnit.Faction, "Unsupported faction")
		};

		private void OnUnitTurnEnded(UnitTurnEndedEvent e)
		{
			this.Log($"Unit '{e.TurnUnitId}' finished acting");

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

			AwaitThen(
				() => _turnService.EndTurn(),
				StartNewTurn,
				cmd => cmd
					.Expect(EPresentationCategory.UI, PresentationType.UI.TurnEnd)
			);

			this.Log($"Turn {turnNumber}{(WaitForPresentation ? " (awaiting presentation)" : "")} ended");
		}

		private void AwaitThen(Action trigger, Action onComplete, Action<AwaitPresentationCommand> configure)
		{
			if (WaitForPresentation)
			{
				var cmd = new AwaitPresentationCommand(onComplete);
				configure?.Invoke(cmd);
				_commandQueue.EnqueueAndExecute(cmd);
				trigger?.Invoke();
			}
			else
			{
				trigger?.Invoke();
				onComplete();
			}
		}
	}
}
