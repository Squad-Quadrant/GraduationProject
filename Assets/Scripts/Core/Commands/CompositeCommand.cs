using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Commands
{
	/// <summary>
	/// A command that contains multiple sub-commands executed in sequence.
	/// </summary>
	public class CompositeCommand : ICommand
	{
		private readonly List<ICommand> _commands = new();
		private int _index;
		private Action _onComplete;
		private bool _isExecuting;

		public string Name { get; }

		/// <summary>
		/// A composite command can only be undone if ALL sub-commands support undo.
		/// </summary>
		public bool CanUndo => _commands.Count != 0 && _commands.All(cmd => cmd.CanUndo);

		public int Count => _commands.Count;

		public CompositeCommand(string name = null) => Name = name ?? "CompositeCommand";

		public CompositeCommand Add(ICommand command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			if (_isExecuting)
				throw new InvalidOperationException("Cannot add commands while executing.");

			_commands.Add(command);
			return this; // Fluent API
		}

		public void Execute(Action onComplete = null)
		{
			if (_isExecuting)
				throw new InvalidOperationException($"CompositeCommand '{Name}' is already executing.");

			if (_commands.Count == 0)
			{
				onComplete?.Invoke();
				return;
			}

			_isExecuting = true;
			_index = 0;
			_onComplete = onComplete;

			ExecuteCurrentCommand();
		}

		public void Undo(Action onComplete = null)
		{
			if (!CanUndo)
				throw new InvalidOperationException($"CompositeCommand '{Name}' does not support undo.");

			if (_commands.Count == 0)
			{
				onComplete?.Invoke();
				return;
			}

			_isExecuting = true;
			_index = _commands.Count - 1; // Undo in reverse order
			_onComplete = onComplete;

			UndoCurrentCommand();
		}

		private void ExecuteCurrentCommand()
		{
			if (_index >= _commands.Count)
			{
				// All commands executed
				_isExecuting = false;
				_onComplete?.Invoke();
				_onComplete = null;
				return;
			}

			var command = _commands[_index];
			command.Execute(() =>
			{
				_index++;
				ExecuteCurrentCommand();
			});
		}

		private void UndoCurrentCommand()
		{
			if (_index < 0)
			{
				// All commands undone
				_isExecuting = false;
				_onComplete?.Invoke();
				_onComplete = null;
				return;
			}

			var command = _commands[_index];
			command.Undo(() =>
			{
				_index--;
				UndoCurrentCommand();
			});
		}
	}
}
