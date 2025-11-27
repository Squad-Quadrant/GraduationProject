using System;

namespace Core.Commands
{
	public interface ICommand
	{
		string Name { get; }
		bool CanUndo { get; }
		void Execute(Action onComplete = null);
		void Undo(Action onComplete = null);
	}
}
