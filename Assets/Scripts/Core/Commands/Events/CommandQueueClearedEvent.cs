using Core.Events;

namespace Core.Commands.Events
{
	public readonly struct CommandQueueClearedEvent : IEvent
	{
		public override string ToString() => "[CommandQueueCleared]";
	}
}
