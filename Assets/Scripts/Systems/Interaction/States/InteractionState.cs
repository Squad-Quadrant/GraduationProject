using System;
using System.Linq;
using Core.Events;
using Core.FSM;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Systems.AreaEffect;
using Systems.Map;
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
			var worldPos = e.WorldPosition;

			if (e.HoveredUnitId != null && ctx.UnitService.TryGetUnit(e.HoveredUnitId, out var unit)) // 单位
			{
				Publish(ctx, CursorInfoEvent.ForUnit(
					unit.position, worldPos,
					unit.name, unit.CurrentHp, unit.maxHp, unit.CurrentDefense, unit.faction));
				return;
			}

			if (!e.CellPosition.HasValue)
			{
				Publish(ctx, CursorInfoEvent.Hide());
				return;
			}

			var cell = e.CellPosition.Value;

			if (!ctx.RegionService.IsCellUnlocked(cell)) // 未解锁
			{
				Publish(ctx, CursorInfoEvent.ForCell(cell, worldPos, "未解锁"));
				return;
			}

			if (!ctx.VisionService.IsCellVisible(cell)) // 无视野
			{
				if (e.HoveredUnitId != null && ctx.VisionService.IsEnemySpotted(e.HoveredUnitId))
				{
					Publish(ctx, CursorInfoEvent.ForSpottedHiddenEnemy(cell, worldPos));
					return;
				}
				Publish(ctx, CursorInfoEvent.ForCell(cell, worldPos, "无视野"));
				return;
			}

			// 普通格
			var mapCell = ctx.MapService.Data.GetCell(cell);
			var statusLine = BuildCellStatusLine(mapCell, ctx.AreaEffectService);
			Publish(ctx, CursorInfoEvent.ForCell(cell, worldPos, statusLine));
		}

		private static string BuildCellStatusLine(MapCell mapCell, IAreaEffectService areaEffectService)
		{
			if (mapCell == null) return "状态: 空";

			var effects = areaEffectService?.GetAt(mapCell.Position);
			if (effects is { Count: > 0 })
			{
				var names = string.Join(", ", effects.Select(ae => ae.Behavior.DisplayName));
				return $"状态: {names}";
			}

			if (mapCell.SceneActor != null)
			{
				var name = !string.IsNullOrEmpty(mapCell.SceneActor.DisplayName)
					? mapCell.SceneActor.DisplayName
					: mapCell.SceneActor.Type.ToString();
				return $"物体: {name}";
			}

			return "状态: 空";
		}
	}
}
