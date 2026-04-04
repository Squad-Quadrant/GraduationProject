using System;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;

namespace Systems.Interaction.States
{
	public class IdleState : InteractionState
	{
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<CellClickedEvent> _onCellClicked;
		private Action<EscInputEvent> _onEsc;
		private Action<PointerHoverEvent> _onPointerHover;

		public IdleState() : base(InteractionStates.Idle) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			this.Log("Entered Idle State");

			_onUnitClicked = OnUnitClicked;
			_onCellClicked = OnCellClicked;
			_onEsc = OnEsc;
			_onPointerHover = OnPointerHover;

			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onEsc);
			Subscribe(ctx, _onPointerHover);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exiting Idle State");

			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onEsc);
			Unsubscribe(ctx, _onPointerHover);

			_onUnitClicked = null;
			_onCellClicked = null;
			_onEsc = null;
			_onPointerHover = null;
			Publish(ctx, CursorInfoEvent.Hide());

			base.OnExit(ctx);
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (!Context.UnitService.TryGetUnit(e.UnitId, out var unit))
			{
				this.LogWarning($"Clicked unit with ID {e.UnitId} not found.");
				return;
			}

			if (Context.CanControlUnit(unit))
				SelectUnit(unit);
			else
			{
				this.Log($"Cannot control unit with ID {e.UnitId}.");
				// todo: provide feedback to the player here
			}
		}

		private void OnCellClicked(CellClickedEvent e) =>
			this.Log($"Empty cell clicked: {e.CellPosition}");

		private void OnEsc(EscInputEvent e)
		{
			this.Log("ESC pressed in Idle → requesting settings panel");
			Publish(Context, new OpenSettingsRequestEvent());
		}

		private void OnPointerHover(PointerHoverEvent e) => PublishBasicCursorInfo(Context, e);

		private void SelectUnit(Unit.Unit unit)
		{
			this.Log($"Selecting unit: {unit.name}");

			Context.selectedUnit = unit;

			Publish(Context, new UnitSelectedEvent(
				unit.id,
				unit.position));

			StateMachine(Context).ChangeState<UnitSelectedState>();
		}
	}
}
