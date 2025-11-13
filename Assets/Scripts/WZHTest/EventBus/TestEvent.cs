using Core.Events;

namespace WZHTest.EventBus
{
	public struct TestEvent : IEvent
	{
		public string Message { get; }

		public TestEvent(string message) => Message = message;
	}
}
