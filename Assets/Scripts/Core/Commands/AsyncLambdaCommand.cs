using System;

namespace Core.Commands
{
	public class AsyncLambdaCommand : AsyncCommand
	{
		private readonly string _name;
		private readonly Action<Action> _action;

		public override string Name => _name;
		public override bool CanUndo => false;

		public AsyncLambdaCommand(string name, Action<Action> action)
		{
			_name = name ?? "AsyncLambdaCommand";
			_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		protected override void OnExecuteAsync() => _action(CompleteExecution);
	}
}
