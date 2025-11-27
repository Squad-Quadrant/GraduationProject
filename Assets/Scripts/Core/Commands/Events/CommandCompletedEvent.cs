using Core.Events;

namespace Core.Commands.Events
{
	public readonly struct CommandCompletedEvent : IEvent
	{
		public ICommand Command { get; }

		public CommandCompletedEvent(ICommand command) => Command = command;

		public override string ToString() => $"[CommandCompleted] {Command.Name}";
	}
}
