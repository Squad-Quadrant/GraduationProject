using System;
using System.Collections.Generic;
using Core.Log;

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

		private readonly ILog _logger;

		public EventBus(ILog logger) => _logger = logger;

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
				_logger.LogWarning($"[Event] [Subscribe] {eventType.Name} already exists");

			var subscription = new Subscription(handler, priority, onlyOnce);
			subscriptions.Add(subscription);
			subscriptions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
			_logger.Log("[Event] Subscribed to event: " + eventType.Name);
		}

		#endregion


		#region Unsubscribe Methods

		public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var eventType = typeof(TEvent);

			if (!_subscriptionDic.TryGetValue(eventType, out var subscriptions))
			{
				_logger.LogWarning($"[Event] [Unsubscribe] {eventType.Name} does not exist");
				return;
			}

			subscriptions.RemoveAll(s => s.Handler.Equals(handler));
			_logger.Log("[Event] Unsubscribed from event: " + eventType.Name);

			if (subscriptions.Count == 0)
				_subscriptionDic.Remove(eventType);
		}

		#endregion


		#region Publish Methods

		public void Publish<TEvent>(TEvent eventData) where TEvent : IEvent
		{
			var eventType = typeof(TEvent);

			if (!_subscriptionDic.TryGetValue(eventType, out var subscriptions))
			{
				_logger.LogWarning($"[Event] [Publish] {eventType.Name} does not exist");
				return;
			}

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
					throw new Exception($"[Event] Error invoking handler for {eventType.Name}", ex);
				}
			}

			_logger.Log($"[Event] Event published: {eventType.Name}");

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
