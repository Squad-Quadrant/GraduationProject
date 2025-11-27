using Core.Events;

namespace Core.Commands.Events
{
	public readonly struct CommandQueueCompletedEvent : IEvent
	{
		public override string ToString() => "[CommandQueueCompleted]";
	}
}
