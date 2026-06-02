using System;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;

namespace Systems.Interaction.States
{
	public class UnitInspectState : InteractionState
	{
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;
		private Action<PointerHoverEvent> _onPointerHover;

		public UnitInspectState() : base(InteractionStates.UnitInspect) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			if (ctx.selectedUnit == null)
				throw new InvalidOperationException("Entered UnitInspectState without a selected unit.");
			if (ctx.inspectedUnit == null)
				throw new InvalidOperationException("Entered UnitInspectState without an inspected unit.");

			this.Log($"Entered - inspecting: {ctx.inspectedUnit.name}");

			Publish(Context, new UnitInspectedEvent(ctx.inspectedUnit.id));

			_onUnitClicked = OnUnitClicked;
			_onBack = OnBack;
			_onEsc = OnEsc;
			_onPointerHover = OnPointerHover;

			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
			Subscribe(ctx, _onPointerHover);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited - resuming control of selected unit");

			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);
			Unsubscribe(ctx, _onPointerHover);
			_onUnitClicked = null;
			_onBack = null;
			_onEsc = null;
			_onPointerHover = null;

			ctx.inspectedUnit = null;
			Publish(ctx, CursorInfoEvent.Hide());
			Publish(ctx, new UnitInspectedEvent(ctx.selectedUnit.id)); // 这里触发一次inspect event，让镜头聚焦回去，一个比较tricky的方法

			base.OnExit(ctx);
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (!Context.UnitService.TryGetUnit(e.UnitId, out var unit))
			{
				this.LogWarning($"Clicked unit '{e.UnitId}' not found.");
				return;
			}

			if (unit.id == Context.selectedUnit.id)
			{
				StateMachine(Context).ChangeState<UnitSelectedState>();
				return;
			}

			if (unit.id == Context.inspectedUnit.id) return;

			Context.inspectedUnit = unit;
			Publish(Context, new UnitInspectedEvent(unit.id));
		}

		private void OnBack(BackInputEvent e) => StateMachine(Context).ChangeState<UnitSelectedState>();
		private void OnEsc(EscInputEvent e) => StateMachine(Context).ChangeState<UnitSelectedState>();

		private void OnPointerHover(PointerHoverEvent e) => PublishBasicCursorInfo(Context, e);
	}
}
