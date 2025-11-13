using System;
using System.Collections.Generic;

namespace Core.Events
{
	public class EventBus : IEventBus
	{
		private class Subscription
		{
			public Delegate Handler { get; }
			public int Priority { get; }
			public bool IsOnce { get; }

			public Subscription(Delegate handler, int priority, bool isOnce)
			{
				Handler = handler;
				Priority = priority;
				IsOnce = isOnce;
			}
		}

		private readonly Dictionary<Type, List<Subscription>> _subscriptionDic = new();
		private readonly List<Subscription> _onceToRemove = new();

		#region Subscribe Methods

		public void Subscribe<TEvent>(Action<TEvent> handler, int priority = 0, bool onlyOnce = false) where TEvent : IEvent
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var eventType = typeof(TEvent);

			if (!_subscriptionDic.TryGetValue(eventType, out var subscriptions))
			{
				subscriptions = new List<Subscription>();
				_subscriptionDic[eventType] = subscriptions;
			}

			if (subscriptions.Exists(s => s.Handler.Equals(handler)))
				throw new Exception($"[Event:Subscribe] {eventType.FullName} already exists");

			var subscription = new Subscription(handler, priority, onlyOnce);
			subscriptions.Add(subscription);
			subscriptions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
		}

		#endregion


		#region Unsubscribe Methods

		public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var eventType = typeof(TEvent);

			if (!_subscriptionDic.TryGetValue(eventType, out var subscriptions))
				throw new Exception($"[Event:Unsubscribe] {eventType.FullName} does not exist");

			subscriptions.RemoveAll(s => s.Handler.Equals(handler));

			if (subscriptions.Count == 0)
				_subscriptionDic.Remove(eventType);
		}

		#endregion


		#region Publish Methods

		public void Publish<TEvent>(TEvent eventData) where TEvent : IEvent
		{
			var eventType = typeof(TEvent);

			if (!_subscriptionDic.TryGetValue(eventType, out var subscriptions))
				throw new Exception($"[Event:Publish] {eventType.FullName} does not exist");

			_onceToRemove.Clear();

			foreach (var subscription in subscriptions)
			{
				try
				{
					var action = (Action<TEvent>)subscription.Handler;
					action.Invoke(eventData);

					if (subscription.IsOnce)
						_onceToRemove.Add(subscription);
				}
				catch (Exception ex)
				{
					throw new Exception($"[Event:Publish] Error invoking handler for {eventType.FullName}", ex);
				}
			}

			foreach (var subscription in _onceToRemove)
				subscriptions.Remove(subscription);

			if (subscriptions.Count == 0)
				_subscriptionDic.Remove(eventType);
		}

		#endregion


		#region Lifetime Management

		public void Clear()
		{
			_subscriptionDic.Clear();
			_onceToRemove.Clear();
		}

		#endregion
	}
}
