using Core.Events;

namespace Data.Runtime.Events
{
	public struct ExampleEvent : IEvent
	{
		public string Msg { get; }

		public ExampleEvent(string msg) => Msg = msg;
	}
}
