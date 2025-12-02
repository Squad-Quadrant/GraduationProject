using System;
using Core.Events;
using Core.FSM;
using UnityEngine;

namespace Systems.Interaction.States
{
	public abstract class InteractionState : State<InteractionContext>
	{
		private const bool EnableLogs = true;

		protected InteractionContext Context { get; set; }

		protected InteractionState(string name) : base(name) {}

		public override void OnEnter(InteractionContext ctx) => Context = ctx;
		public override void OnExit(InteractionContext ctx) => Context = null;

		protected StateMachine<InteractionContext> StateMachine(InteractionContext ctx) => ctx.StateMachine;

		protected void Subscribe<TEvent>(InteractionContext ctx, Action<TEvent> handler, int priority = 0) where TEvent : IEvent
			=> ctx.EventBus.Subscribe(handler, priority);

		protected void Unsubscribe<TEvent>(InteractionContext ctx, Action<TEvent> handler) where TEvent : IEvent
			=> ctx.EventBus.Unsubscribe(handler);

		protected void Publish<TEvent>(InteractionContext ctx, TEvent evt) where TEvent : IEvent
			=> ctx.EventBus.Publish(evt);

		#region Debug

		protected static void Log(string message)
		{
			if (EnableLogs) Debug.Log($"{message}");
		}

		protected static void LogWarning(string message)
		{
			if (EnableLogs) Debug.LogWarning($"{message}");
		}

		protected static void LogError(string message)
		{
			if (EnableLogs) Debug.LogError($"{message}");
		}

		#endregion
	}
}
