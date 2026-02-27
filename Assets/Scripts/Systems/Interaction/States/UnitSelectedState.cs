using System;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;

namespace Systems.Interaction.States
{
	public class UnitSelectedState : InteractionState
	{
		private Action<ActionSelectedEvent> _onActionSelected;
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;

		public UnitSelectedState() : base(InteractionStates.UnitSelected) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			this.Log($"Entered - Unit: {ctx.selectedUnit?.name}");

			if (ctx.selectedUnit == null)
			{
				this.LogError("No unit selected! Returning to Idle.");
				ctx.StateMachine.ChangeState(new IdleState());
				return;
			}

			_onActionSelected = OnActionSelected;
			_onUnitClicked = OnUnitClicked;
			_onBack = OnBack;
			_onEsc = OnEsc;
			Subscribe(ctx, _onActionSelected);
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Unsubscribe(ctx, _onActionSelected);
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);
			_onActionSelected = null;
			_onUnitClicked = null;
			_onBack = null;
			_onEsc = null;

			base.OnExit(ctx);
		}

		private void OnActionSelected(ActionSelectedEvent e)
		{
			this.Log($"Action selected: {e.ActionType}");

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
					this.LogWarning($"Unhandled action: {e.ActionType}");
					break;
			}
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (e.UnitId == Context.selectedUnit?.id)
				return;

			if (!Context.UnitService.TryGetUnit(e.UnitId, out var unit))
			{
				this.LogWarning($"Clicked unit with ID {e.UnitId} not found.");
				return;
			}

			if (Context.CanControlUnit(unit))
				SwitchToUnit(unit);
			else
			{
				this.Log($"Cannot control unit with ID {e.UnitId}.");
				// todo: provide feedback to the player here
			}
		}

		private void OnBack(BackInputEvent e)
		{
			this.Log("Back input received - Deselecting unit and going idle");
			DeselectAndGoIdle();
		}

		private void OnEsc(EscInputEvent e)
		{
			this.Log("Esc input received - Deselecting unit and going idle");
			DeselectAndGoIdle();
		}

		private void SwitchToUnit(Unit.Unit newUnit)
		{
			this.Log($"Switching to unit: {newUnit.name}");

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
				newUnit.Position,
				Context.availableActions
			));
		}

		private void DeselectAndGoIdle()
		{
			var unitId = Context.selectedUnit?.id;
			Publish(Context, new UnitDeselectedEvent(unitId));
			StateMachine(Context).ChangeState<IdleState>();
		}

		private void ExecuteWait()
		{
			// todo: need more logic here
			this.Log("Executing Wait action");

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
			this.Log("Executing EndTurn action");

			Context.TurnService.EndTurn();
			Context.StateMachine.ChangeState(new IdleState());
		}
	}
}
