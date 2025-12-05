using System;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.View;
using Systems.Turn;
using Systems.Unit;

namespace Systems.GamePlay
{
	public class GameServer : IGameServer, IDisposable
	{
		public bool IsRunning { get; private set; }
		public bool WaitForPresentation { get; set; } = true;

		private readonly ITurnService _turnService;
		private readonly IUnitService _unitService;
		private readonly IEventBus _eventBus;
		private readonly ICommandQueue _commandQueue;

		public GameServer(
			ITurnService turnService,
			IUnitService unitService,
			IEventBus eventBus,
			ICommandQueue commandQueue)
		{
			_turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_commandQueue = commandQueue ?? throw new ArgumentNullException(nameof(commandQueue));

			_eventBus.Subscribe<UnitTurnEndedEvent>(OnUnitTurnEnded);
			_eventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
			this.Log("Initialized");
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

			_turnService.StartTurn();
		}

		private void OnUnitTurnEnded(UnitTurnEndedEvent e)
		{
			this.Log($"Unit '{e.UnitId}' finished. Processing transition...");

			if (WaitForPresentation)
			{
				_commandQueue.EnqueueAndExecute(
					new AwaitPresentationCommand(_eventBus, onComplete: ProcessNextUnit)
						.Expect(EPresentationCategory.UI, PresentationType.UI.TurnBanner) // todo: Need specific type
					);
			}
			else
				ProcessNextUnit();
		}

		private void ProcessNextUnit()
		{
			if (_turnService.IsCurrentTurnComplete())
			{
				this.Log("Queue empty, ending turn");
				_turnService.EndTurn();
			}
			else
			{
				this.Log("Proceeding to next unit");
				_turnService.NextUnit();
			}
		}

		private void OnTurnEnded(TurnEndedEvent e)
		{
			this.Log($"Turn {e.TurnNumber} ended");

			// Check win/lose conditions
			// if (CheckGameOver()) return;

			// Auto-start next turn
			this.Log("Starting next turn...");

			if (WaitForPresentation)
			{
				_commandQueue.EnqueueAndExecute(
					new AwaitPresentationCommand(_eventBus, onComplete: () => _turnService.StartTurn())
						.Expect(EPresentationCategory.UI, PresentationType.UI.TurnBanner) // todo: Need specific type
					);
			}
			else
				_turnService.StartTurn();
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitTurnEndedEvent>(OnUnitTurnEnded);
			_eventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
			IsRunning = false;
		}
	}
}
