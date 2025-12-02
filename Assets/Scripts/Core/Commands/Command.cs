using System;

namespace Core.Commands
{
	/// <summary>
	/// Sync command base class
	/// Most sync commands can inherit from this class
	/// </summary>
	public abstract class SyncCommand : ICommand
	{
		public virtual string Name => GetType().Name;

		public virtual bool CanUndo => false;

		public void Execute(Action onComplete = null)
		{
			OnExecute();
			onComplete?.Invoke();
		}

		public void Undo(Action onComplete = null)
		{
			if (!CanUndo)
				throw new InvalidOperationException($"[Command] Command '{Name}' does not support undo operation.");
			OnUndo();
			onComplete?.Invoke();
		}

		protected abstract void OnExecute();

		protected virtual void OnUndo() { }
	}

	public abstract class AsyncCommand : ICommand
	{
		public virtual string Name => GetType().Name;

		public virtual bool CanUndo => false;

		/// <summary>
		/// Indicates whether the command is currently executing for debugging and state verification.
		/// </summary>
		public bool IsExecuting { get; private set; }

		private Action _onExecuteComplete;
		private Action _onUndoComplete;

		public void Execute(Action onComplete = null)
		{
			if (IsExecuting)
				throw new InvalidOperationException($"[Command] Command '{Name}' already executing.");
			IsExecuting = true;
			_onExecuteComplete = onComplete;
			OnExecuteAsync();
		}

		public void Undo(Action onComplete = null)
		{
			if (!CanUndo)
				throw new InvalidOperationException($"[Command] Command '{Name}' does not support undo operation.");
			if (IsExecuting)
				throw new InvalidOperationException($"[Command] Command '{Name}' already executing.");
			IsExecuting = true;
			_onUndoComplete = onComplete;
			OnUndoAsync();
		}

		/// <summary>
		/// Override this method to start the async execution.
		/// CompleteExecution() should be called manually when execution is finished.
		/// </summary>
		protected abstract void OnExecuteAsync();

		/// <summary>
		/// Override this method to start the async undo operation.
		/// CompleteUndo() should be called manually when undo is finished.
		/// </summary>
		protected virtual void OnUndoAsync() => CompleteUndo();

		/// <summary>
		/// Call this method when async execution completes.
		/// This notifies the CommandQueue to proceed with the next command.
		/// </summary>
		protected void CompleteExecution()
		{
			IsExecuting = false;
			_onExecuteComplete?.Invoke();
			_onExecuteComplete = null;
		}

		/// <summary>
		/// Call this method when async undo completes.
		/// </summary>
		protected void CompleteUndo()
		{
			IsExecuting = false;
			_onUndoComplete?.Invoke();
			_onUndoComplete = null;
		}
	}
}
