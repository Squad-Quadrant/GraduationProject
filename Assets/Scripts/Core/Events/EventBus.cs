using System;
using System.Collections.Generic;
using System.Linq;
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

		#region Debugging

		public bool RecordHistory { get; set; } = true;

		public int MaxHistorySize { get; set; } = 50;

		private readonly List<EventHistoryEntry> _eventHistory = new();

		public readonly struct SubscriptionInfo
		{
			public Type EventType { get; }
			public int SubscriberCount { get; }
			public int OnceCount { get; }

			public SubscriptionInfo(Type eventType, int subscriberCount, int onceCount)
			{
				EventType = eventType;
				SubscriberCount = subscriberCount;
				OnceCount = onceCount;
			}
		}

		public readonly struct EventHistoryEntry
		{
			public DateTime Timestamp { get; }
			public string EventTypeName { get; }
			public string EventData { get; }
			public int HandlerCount { get; }

			public EventHistoryEntry(DateTime timestamp, string eventTypeName, string eventData, int handlerCount)
			{
				Timestamp = timestamp;
				EventTypeName = eventTypeName;
				EventData = eventData;
				HandlerCount = handlerCount;
			}
		}

		public IReadOnlyList<SubscriptionInfo> GetSubscriptionInfos()
		{
			return _subscriptionDic.Select(kvp => new SubscriptionInfo(
				kvp.Key,
				kvp.Value.Count,
				kvp.Value.Count(s => s.IsOnce)
			)).ToList();
		}

		public IReadOnlyList<EventHistoryEntry> GetEventHistory() => _eventHistory;

		public void ClearHistory() => _eventHistory.Clear();

		public int SubscribedEventTypeCount => _subscriptionDic.Count;

		public int TotalSubscriptionCount => _subscriptionDic.Values.Sum(list => list.Count);

		#endregion


		#region Subscribe

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
				this.LogWarning($"[Subscribe] {eventType.Name} already exists");

			var subscription = new Subscription(handler, priority, onlyOnce);
			subscriptions.Add(subscription);
			subscriptions.Sort((a, b) => b.Priority.CompareTo(a.Priority));
			this.Log("Subscribed to event: " + eventType.Name);
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
				this.LogWarning($"[Unsubscribe] {eventType.Name} does not exist");
				return;
			}

			subscriptions.RemoveAll(s => s.Handler.Equals(handler));
			this.Log("Unsubscribed from event: " + eventType.Name);
		}

		#endregion

		#region Publish Methods

		public void Publish<TEvent>(TEvent eventData) where TEvent : IEvent
		{
			var eventType = typeof(TEvent);
			var handlerCount = 0;

			if (_subscriptionDic.TryGetValue(eventType, out var subscriptions))
			{
				_onceToRemove.Clear();
				handlerCount = subscriptions.Count;

				var subscriptionsCopy = subscriptions.ToList();
				foreach (var subscription in subscriptionsCopy)
				{
#if !UNITY_EDITOR
					try
					{
#endif
						var action = (Action<TEvent>)subscription.Handler;
						action.Invoke(eventData);

						if (subscription.IsOnce)
							_onceToRemove.Add(subscription);
#if !UNITY_EDITOR
					}
					catch (Exception ex)
					{
						throw new Exception($"Error invoking handler for {eventType.Name}", ex);
					}
#endif
				}

				foreach (var subscription in _onceToRemove)
					subscriptions.Remove(subscription);
			}

			if (RecordHistory)
			{
				var entry = new EventHistoryEntry(
					DateTime.Now,
					eventType.Name,
					eventData.ToString(),
					handlerCount
				);

				_eventHistory.Insert(0, entry); // Most recent first

				// Trim history if exceeds max size
				while (_eventHistory.Count > MaxHistorySize)
					_eventHistory.RemoveAt(_eventHistory.Count - 1);
			}

			this.LogDebug($"Event published: {eventType.Name}");
		}

		#endregion

		#region Lifetime Management

		public void Clear()
		{
			_subscriptionDic.Clear();
			_onceToRemove.Clear();
			_eventHistory.Clear();
		}

		#endregion
	}
}
