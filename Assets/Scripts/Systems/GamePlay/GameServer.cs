using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Turn;
using Systems.Turn;
using Systems.Unit;

namespace Systems.GamePlay
{
	public class GameServer : IGameServer, IDisposable
	{
		public bool IsRunning { get; private set; }

		private readonly ITurnService _turnService;
		private readonly IUnitService _unitService;
		private readonly IEventBus _eventBus;

		public GameServer(ITurnService turnService, IUnitService unitService, IEventBus eventBus)
		{
			_turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

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
			this.Log($"Unit '{e.UnitId}' finished. Checking queue...");

			// Check win/lose conditions here if needed
			// if (CheckGameOver()) return;

			if (_turnService.IsCurrentTurnComplete())
			{
				// Queue is empty - end the turn
				this.Log("Queue empty, ending turn");
				_turnService.EndTurn();
			}
			else
			{
				// More units to act - proceed to next
				this.Log("More units in queue, proceeding to next");
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
