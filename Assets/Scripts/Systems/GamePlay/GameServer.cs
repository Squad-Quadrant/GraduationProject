using System;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.View;
using Systems.Turn;

namespace Systems.GamePlay
{
	public class GameServer : IGameServer, IDisposable
	{
		public bool IsRunning { get; private set; }
		public bool WaitForPresentation { get; set; } = true;

		private readonly ITurnService _turnService;
		private readonly IEventBus _eventBus;
		private readonly ICommandQueue _commandQueue;

		public GameServer(
			ITurnService turnService,
			IEventBus eventBus,
			ICommandQueue commandQueue)
		{
			_turnService = turnService;
			_eventBus = eventBus;
			_commandQueue = commandQueue;

			_eventBus.Subscribe<UnitTurnEndedEvent>(OnUnitTurnEnded);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitTurnEndedEvent>(OnUnitTurnEnded);
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

			StartNewTurn();
		}

		private void StartNewTurn()
		{
			_turnService.StartTurn();

			this.Log($"Turn {_turnService.TurnNumber}{(WaitForPresentation ? " (awaiting presentation)" : "")} started");

			AwaitThen(AdvanceToNextUnit, cmd => cmd
				.Expect(EPresentationCategory.UI, PresentationType.UI.TurnStart)
			);
		}

		private void AdvanceToNextUnit()
		{
			var unit = _turnService.NextUnit();

			if (unit != null) // Found next unit to act
			{
				this.Log($"Unit '{unit.Id}' is now acting");
				return;
			}

			// Queue exhausted — end the turn
			this.Log("No more actionable units, ending turn");
			EndCurrentTurn();
		}

		private void OnUnitTurnEnded(UnitTurnEndedEvent e)
		{
			this.Log($"Unit '{e.UnitId}' finished acting");

			AwaitThen(ProcessAfterUnitTurn, cmd => cmd
				.Expect(EPresentationCategory.UI, PresentationType.UI.UnitTransition)
			);
		}

		private void ProcessAfterUnitTurn()
		{
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
