using System;
using Core.Events;
using Core.FSM;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using UnityEngine;

namespace Systems.Interaction.States
{
	public abstract class InteractionState : State<InteractionContext>
	{
		protected InteractionContext Context { get; private set; }

		protected InteractionState(string name) : base(name) {}

		public override void OnEnter(InteractionContext ctx) => Context = ctx;
		public override void OnExit(InteractionContext ctx) => Context = null;

		protected StateMachine<InteractionContext> StateMachine(InteractionContext ctx) => ctx.StateMachine;

		protected static void Subscribe<TEvent>(InteractionContext ctx, Action<TEvent> handler, int priority = 0) where TEvent : IEvent
			=> ctx.EventBus.Subscribe(handler, priority);

		protected static void Unsubscribe<TEvent>(InteractionContext ctx, Action<TEvent> handler) where TEvent : IEvent
			=> ctx.EventBus.Unsubscribe(handler);

		protected static void Publish<TEvent>(InteractionContext ctx, TEvent evt) where TEvent : IEvent
			=> ctx.EventBus.Publish(evt);

		protected static void PublishBasicCursorInfo(InteractionContext ctx, PointerHoverEvent e)
		{
			if (!e.CellPosition.HasValue)
			{
				Publish(ctx, CursorInfoEvent.Hide());
				return;
			}

			var cell = e.CellPosition.Value;
			var terrainName = ctx.MapService.Data.GetCell(cell)?.Terrain.ToString() ?? "";

			if (e.HoveredUnitId != null && ctx.UnitService.TryGetUnit(e.HoveredUnitId, out var unit))
			{
				Publish(ctx, CursorInfoEvent.ForUnit(
					cell, e.WorldPosition, terrainName,
					unit.name, unit.CurrentHp, unit.maxHp, unit.faction.ToString()));
				return;
			}

			Publish(ctx, CursorInfoEvent.ForTerrain(cell, e.WorldPosition, terrainName));
		}
	}
}
