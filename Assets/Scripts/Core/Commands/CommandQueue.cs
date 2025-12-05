using System;
using System.Collections.Generic;
using Core.Commands.Events;
using Core.Events;
using Core.Log;

namespace Core.Commands
{
	public class CommandQueue : ICommandQueue
	{
		private readonly Queue<ICommand> _pendingCommands = new();
		private readonly Stack<ICommand> _executedCommands = new();  // For undo
		private readonly Stack<ICommand> _undoneCommands = new();    // For redo
		private readonly IEventBus _eventBus;

		private ICommand _currentCommand;
		private bool _isExecuting;
		private bool _isPaused;

		/// <summary>
		/// Number of commands waiting to be executed.
		/// </summary>
		public int PendingCount => _pendingCommands.Count;

		/// <summary>
		/// Number of commands that have been executed and can be undone.
		/// </summary>
		public int UndoCount => _executedCommands.Count;

		/// <summary>
		/// Number of commands that have been undone and can be redone.
		/// </summary>
		public int RedoCount => _undoneCommands.Count;

		/// <summary>
		/// True if a command is currently being executed.
		/// </summary>
		public bool IsExecuting => _isExecuting;

		/// <summary>
		/// True if queue execution is paused.
		/// </summary>
		public bool IsPaused => _isPaused;

		/// <summary>
		/// True if there are no pending commands and no command is executing.
		/// </summary>
		public bool IsIdle => !_isExecuting && _pendingCommands.Count == 0;

		public CommandQueue(IEventBus eventBus)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
		}

		/// <summary>
		/// Adds a command to the end of the queue.
		/// Does not start execution automatically.
		/// </summary>
		public void Enqueue(ICommand command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));
			_pendingCommands.Enqueue(command);
			_undoneCommands.Clear();
			this.Log($"Enqueued: {command.Name} (Pending: {PendingCount})");
		}

		/// <summary>
		/// Adds a command and immediately starts execution if not already running.
		/// A convenience method combining Enqueue + ExecuteAll.
		/// </summary>
		public void EnqueueAndExecute(ICommand command)
		{
			Enqueue(command);

			if (!_isExecuting && !_isPaused)
				ExecuteNext();
		}

		/// <summary>
		/// Starts executing all pending commands sequentially.
		/// If already executing, this method does nothing.
		/// </summary>
		public void ExecuteAll()
		{
			if (_isExecuting)
			{
				this.Log("Already executing, ignoring ExecuteAll call.");
				return;
			}

			if (_isPaused)
			{
				this.Log("Queue is paused, ignoring ExecuteAll call.");
				return;
			}

			ExecuteNext();
		}

		/// <summary>
		/// Undoes the last executed command.
		/// </summary>
		/// <returns>True if undo was performed, false if nothing to undo.</returns>
		public bool Undo()
		{
			if (_isExecuting)
			{
				this.LogWarning("Cannot undo while executing.");
				return false;
			}

			if (_executedCommands.Count == 0)
			{
				this.Log("Nothing to undo.");
				return false;
			}

			var command = _executedCommands.Peek();

			if (!command.CanUndo)
			{
				this.LogWarning($"Command '{command.Name}' does not support undo.");
				return false;
			}

			_executedCommands.Pop();
			_isExecuting = true;

			this.Log($"Undoing: {command.Name}");

			command.Undo(() =>
			{
				_isExecuting = false;
				_undoneCommands.Push(command);
				this.Log($"Undo completed: {command.Name}");
			});

			return true;
		}

		/// <summary>
		/// Redoes the last undone command.
		/// </summary>
		/// <returns>True if redo was performed, false if nothing to redo.</returns>
		public bool Redo()
		{
			if (_isExecuting)
			{
				this.LogWarning("Cannot redo while executing.");
				return false;
			}

			if (_undoneCommands.Count == 0)
			{
				this.Log("Nothing to redo.");
				return false;
			}

			var command = _undoneCommands.Pop();
			_isExecuting = true;

			this.Log($"Redoing: {command.Name}");
			_eventBus.Publish(new CommandStartedEvent(command));

			command.Execute(() =>
			{
				_isExecuting = false;
				_currentCommand = null;
				_executedCommands.Push(command);

				this.Log($"Redo completed: {command.Name}");
				_eventBus.Publish(new CommandCompletedEvent(command));
			});

			return true;
		}

		/// <summary>
		/// Pauses queue execution after the current command completes.
		/// </summary>
		public void Pause()
		{
			_isPaused = true;
			this.Log("Paused.");
		}

		/// <summary>
		/// Resumes queue execution.
		/// </summary>
		public void Resume()
		{
			if (!_isPaused) return;

			_isPaused = false;
			this.Log("Resumed.");

			if (!_isExecuting && _pendingCommands.Count > 0)
				ExecuteNext();
		}

		/// <summary>
		/// Clears all pending commands. Does not affect the currently executing command.
		/// </summary>
		public void Clear()
		{
			var count = _pendingCommands.Count;
			_pendingCommands.Clear();

			this.Log($"Cleared {count} pending commands.");
			_eventBus.Publish(new CommandQueueClearedEvent());
		}

		/// <summary>
		/// Clears all state including executed history.
		/// </summary>
		public void ClearAll()
		{
			Clear();
			_executedCommands.Clear();
			_undoneCommands.Clear();

			this.Log("Cleared all history.");
		}

		private void ExecuteNext()
		{
			if (_pendingCommands.Count == 0)
			{
				this.Log("All commands executed.");
				_eventBus.Publish(new CommandQueueCompletedEvent());
				return;
			}

			if (_isPaused)
			{
				this.Log("Execution paused, waiting for resume.");
				return;
			}

			_currentCommand = _pendingCommands.Dequeue();
			_isExecuting = true;
			this.Log($"Executing: {_currentCommand.Name} (Remaining: {PendingCount})");
			_eventBus.Publish(new CommandStartedEvent(_currentCommand));

			_currentCommand.Execute(() =>
			{
				var completed = _currentCommand;
				_currentCommand = null;
				_isExecuting = false;

				// Only add to history if command supports undo
				if (completed.CanUndo)
					_executedCommands.Push(completed);

				this.Log($"Completed: {completed.Name}");
				_eventBus.Publish(new CommandCompletedEvent(completed));

				// Continue with next command
				ExecuteNext();
			});
		}
	}
}
