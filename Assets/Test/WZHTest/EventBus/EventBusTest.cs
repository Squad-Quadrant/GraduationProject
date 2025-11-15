using Core.Events;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Test.WZHTest.EventBus
{
	public class EventBusTest : MonoBehaviour
	{
		private IEventBus _eventBus;

		[Button]
		private void ResolveEventBus()
		{
			_eventBus = RootContainer.Instance.Resolve<IEventBus>();
			if (_eventBus == null)
				Debug.LogError("Failed to resolve IEventBus from RootContainer.");
			else
				Debug.Log("Successfully resolved IEventBus from RootContainer.");
		}

		[Button]
		private void NullSubscribe() => _eventBus.Subscribe<IEvent>(null);

		[Button]
		private void SubscribeTestEvent()
		{
			_eventBus.Subscribe<TestEvent>(OnTestEventReceived);
		}

		[Button]
		private void PublishTestEvent()
		{
			var testEvent = new TestEvent("Hello from EventBusTest!");
			_eventBus.Publish(testEvent);
		}

		private static void OnTestEventReceived(TestEvent obj)
		{
			Debug.Log($"TestEvent received with message: {obj.Message}");
		}
	}
}
