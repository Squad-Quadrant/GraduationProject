using System;
using System.Collections.Generic;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;

namespace Systems.Interaction.States
{
	public class IdleState : InteractionState
	{
		// for event unsubscription
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<CellClickedEvent> _onCellClicked;
		private Action<EscInputEvent> _onEsc;

		public IdleState() : base(InteractionStates.Idle) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			this.Log("Entered Idle State");

			ctx.ClearSelection(); // Clear any selected units or UI elements
			Publish(ctx, new UnitDeselectedEvent(null));

			_onUnitClicked = OnUnitClicked;
			_onCellClicked = OnCellClicked;
			_onEsc = OnEsc;
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onEsc);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exiting Idle State");

			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onEsc);
			_onUnitClicked = null;
			_onCellClicked = null;
			_onEsc = null;

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

		private void SelectUnit(Unit.Unit unit)
		{
			this.Log($"Selecting unit: {unit.name}");

			Context.selectedUnit = unit;

			Context.availableActions.Clear();
			Context.availableActions.AddRange(unit.GetAvailableActions());

			Publish(Context, new UnitSelectedEvent(
				unit.id,
				unit.Position,
				Context.availableActions
				));

			StateMachine(Context).ChangeState<UnitSelectedState>();
		}
	}
}
