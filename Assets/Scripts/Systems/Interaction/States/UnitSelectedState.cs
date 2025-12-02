using System;
using Data.Runtime;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;

namespace Systems.Interaction.States
{
	public class UnitSelectedState : InteractionState
	{
		private Action<ActionSelectedEvent> _onActionSelected;
		private Action<ActionCancelledEvent> _onActionCancelled;
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<CellClickedEvent> _onCellClicked;

		public UnitSelectedState() : base(InteractionStates.UnitSelected) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			Log($"[UnitSelectedState] Entered - Unit: {ctx.selectedUnit?.name}");

			if (ctx.selectedUnit == null)
			{
				LogError("[UnitSelectedState] No unit selected! Returning to Idle.");
				ctx.StateMachine.ChangeState(new IdleState());
				return;
			}

			_onActionSelected = OnActionSelected;
			_onActionCancelled = OnActionCancelled;
			_onUnitClicked = OnUnitClicked;
			_onCellClicked = OnCellClicked;
			Subscribe(ctx, _onActionSelected);
			Subscribe(ctx, _onActionCancelled);
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onCellClicked);
		}

		public override void OnExit(InteractionContext ctx)
		{
			Log("[UnitSelectedState] Exited");

			Unsubscribe(ctx, _onActionSelected);
			Unsubscribe(ctx, _onActionCancelled);
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onCellClicked);

			_onActionSelected = null;
			_onActionCancelled = null;
			_onUnitClicked = null;
			_onCellClicked = null;

			base.OnExit(ctx);
		}

		private void OnActionSelected(ActionSelectedEvent e)
		{
			Log($"[UnitSelectedState] Action selected: {e.ActionType}");

			Context.currentAction = e.ActionType;

			switch (e.ActionType)
			{
				case EActionType.Move:
					StateMachine(Context).ChangeState<MovementPreviewState>();
					break;

				case EActionType.Attack:
					//todo: To be implemented
					break;

				case EActionType.Wait:
					ExecuteWait();
					break;

				case EActionType.EndTurn:
					ExecuteEndTurn();
					break;

				case EActionType.None:
				case EActionType.Interact:
				case EActionType.UseItem:
				case EActionType.Defend:
				default:
					LogWarning($"[UnitSelectedState] Unhandled action: {e.ActionType}");
					break;
			}
		}

		private void OnActionCancelled(ActionCancelledEvent e)
		{
			Log("[UnitSelectedState] Action cancelled, returning to Idle");
			DeselectAndGoIdle();
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			switch (e.MouseButton)
			{
				case 0: // Left-click

					if (e.UnitId == Context.selectedUnit?.id)
						return;

					if (!Context.UnitService.TryGetUnit(e.UnitId, out var unit))
					{
						LogWarning($"[UnitSelectedState] Clicked unit with ID {e.UnitId} not found.");
						return;
					}

					if (Context.CanControlUnit(unit))
						SwitchToUnit(unit);
					else
					{
						Log($"[UnitSelectedState] Cannot control unit with ID {e.UnitId}.");
						// todo: provide feedback to the player here
					}

					break;
			}
		}

		private void OnCellClicked(CellClickedEvent e)
		{
			switch (e.MouseButton)
			{
				case 1: // Right-click
					Log($"[UnitSelectedState] Empty cell clicked: {e.CellPosition}");
					DeselectAndGoIdle();
					return;
			}
		}

		private void SwitchToUnit(Unit.Unit newUnit)
		{
			Log($"[UnitSelectedState] Switching to unit: {newUnit.name}");

			// Publish deselection of current unit
			Publish(Context, new UnitDeselectedEvent(Context.selectedUnit?.id));

			// Update selection
			Context.selectedUnit = newUnit;
			Context.availableActions.Clear();

			// Calculate new available actions
			Context.availableActions.AddRange(newUnit.GetAvailableActions());

			// Publish new selection
			Publish(Context, new UnitSelectedEvent(
				newUnit.id,
				newUnit.position,
				Context.availableActions
			));
		}

		private void DeselectAndGoIdle()
		{
			var unitId = Context.selectedUnit?.id;
			Publish(Context, new UnitDeselectedEvent(unitId));
			Context.StateMachine.ChangeState<IdleState>();
		}

		private void ExecuteWait()
		{
			// todo: need more logic here
			Log("[UnitSelectedState] Executing Wait action");

			Context.TurnService.EndUnitTurn();

			// Check if there are more units to act
			var nextUnit = Context.TurnService.NextUnit();
			if (nextUnit != null)
			{
				// Auto-select next unit if it's controllable
				if (Context.UnitService.TryGetUnit(nextUnit.Id, out var unit) &&
				    Context.CanControlUnit(unit))
					SwitchToUnit(unit);
				else
					// Next unit is AI-controlled
					Context.StateMachine.ChangeState<IdleState>();
			}
			else
				// No more units - turn might be ending
				Context.StateMachine.ChangeState<IdleState>();
		}

		private void ExecuteEndTurn()
		{
			Log("[UnitSelectedState] Executing EndTurn action");

			Context.TurnService.EndTurn();
			Context.StateMachine.ChangeState(new IdleState());
		}
	}
}
