namespace Core.Commands
{
	public interface ICommandQueue
	{
		public interface ICommandQueue
	{
		/// <summary>
		/// Number of commands waiting to be executed.
		/// </summary>
		int PendingCount { get; }

		/// <summary>
		/// Number of commands that have been executed and can be undone.
		/// </summary>
		int UndoCount { get; }

		/// <summary>
		/// Number of commands that have been undone and can be redone.
		/// </summary>
		int RedoCount { get; }

		/// <summary>
		/// True if a command is currently being executed.
		/// </summary>
		bool IsExecuting { get; }

		/// <summary>
		/// True if queue execution is paused.
		/// </summary>
		bool IsPaused { get; }

		/// <summary>
		/// True if there are no pending commands and no command is executing.
		/// </summary>
		bool IsIdle { get; }

		/// <summary>
		/// Adds a command to the end of the queue.
		/// Does not start execution automatically.
		/// </summary>
		void Enqueue(ICommand command);

		/// <summary>
		/// Adds a command and immediately starts execution if not already running.
		/// A convenience method combining Enqueue + ExecuteAll.
		/// </summary>
		void EnqueueAndExecute(ICommand command);

		/// <summary>
		/// Starts executing all pending commands sequentially.
		/// If already executing, this method does nothing.
		/// </summary>
		void ExecuteAll();

		/// <summary>
		/// Undoes the last executed command.
		/// </summary>
		/// <returns>True if undo was performed, false if nothing to undo.</returns>
		bool Undo();

		/// <summary>
		/// Redoes the last undone command.
		/// </summary>
		/// <returns>True if redo was performed, false if nothing to redo.</returns>
		bool Redo();

		/// <summary>
		/// Pauses queue execution after the current command completes.
		/// </summary>
		void Pause();

		/// <summary>
		/// Resumes queue execution.
		/// </summary>
		void Resume();

		/// <summary>
		/// Clears all pending commands. Does not affect the currently executing command.
		/// </summary>
		void Clear();

		/// <summary>
		/// Clears all state including executed history.
		/// </summary>
		void ClearAll();
	}
	}
}
