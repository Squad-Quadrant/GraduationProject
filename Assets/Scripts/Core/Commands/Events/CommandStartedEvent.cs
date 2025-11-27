using Core.Events;

namespace Core.Commands.Events
{
	public readonly struct CommandStartedEvent : IEvent
	{
		public ICommand Command { get; }

		public CommandStartedEvent(ICommand command) => Command = command;

		public override string ToString() => $"[CommandStarted] {Command.Name}";
	}
}
