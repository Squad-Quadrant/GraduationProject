using System;
using System.Collections.Generic;
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

		public IdleState() : base(InteractionStates.Idle) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			Log("[IdleState] Entered Idle State");

			ctx.ClearSelection(); // Clear any selected units or UI elements
			Publish(ctx, new UnitDeselectedEvent(null));

			_onUnitClicked = OnUnitClicked;
			_onCellClicked = OnCellClicked;
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onCellClicked);
		}

		public override void OnExit(InteractionContext ctx)
		{
			Log("[IdleState] Exiting Idle State");

			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onCellClicked);

			_onUnitClicked = null;
			_onCellClicked = null;

			base.OnExit(ctx);
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			switch (e.MouseButton)
			{
				case 0: // Left-click

					if (!Context.UnitService.TryGetUnit(e.UnitId, out var unit))
					{
						LogWarning($"[IdleState] Clicked unit with ID {e.UnitId} not found.");
						return;
					}

					if (Context.CanControlUnit(unit))
						SelectUnit(unit);
					else
					{
						Log($"[IdleState] Cannot control unit with ID {e.UnitId}.");
						// todo: provide feedback to the player here
					}

					break;
			}
		}

		private void OnCellClicked(CellClickedEvent e) =>
			Log($"[IdleState] Empty cell clicked: {e.CellPosition}");

		private void SelectUnit(Unit.Unit unit)
		{
			Log($"[IdleState] Selecting unit: {unit.name}");

			Context.selectedUnit = unit;

			Context.availableActions.Clear();
			Context.availableActions.AddRange(unit.GetAvailableActions());

			Publish(Context, new UnitSelectedEvent(
				unit.id,
				unit.position,
				Context.availableActions
				));

			StateMachine(Context).ChangeState<UnitSelectedState>();
		}
	}
}
