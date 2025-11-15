using Core.Events;

namespace Test.WZHTest.EventBus
{
	public struct TestEvent : IEvent
	{
		public string Message { get; }

		public TestEvent(string message) => Message = message;
	}
}
