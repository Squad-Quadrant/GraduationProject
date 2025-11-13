using System;

namespace Core.Events
{
	public interface IEventBus
	{
		/// <param name="handler"></param>
		/// <param name="priority">Bigger number means higher priority</param>
		/// <param name="onlyOnce">If true, the handler will be removed after being invoked once.</param>
		void Subscribe<TEvent>(Action<TEvent> handler, int priority = 0, bool onlyOnce = false) where TEvent : IEvent;

		void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;

		void Publish<TEvent>(TEvent eventData) where TEvent : IEvent;

		void Clear();
	}
}
