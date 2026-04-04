using System;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Commands;
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
		private Action<PointerHoverEvent> _onPointerHover;

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
			_onPointerHover = OnPointerHover;
            
			Subscribe(ctx, _onActionSelected);
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
			Subscribe(ctx, _onPointerHover);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Unsubscribe(ctx, _onActionSelected);
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);
			Unsubscribe(ctx, _onPointerHover);

			_onActionSelected = null;
			_onUnitClicked = null;
			_onBack = null;
			_onEsc = null;
			_onPointerHover = null;

			Publish(ctx, CursorInfoEvent.Hide());

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
                case EActionType.TacticalItem0:
                case EActionType.TacticalItem1:
                case EActionType.TacticalItem2:
                    StateMachine(Context).ChangeState<AttackPreviewState>();
					break;

				case EActionType.Wait:
					ExecuteWait();
					break;
                case EActionType.Reload:
                    var reloadCommand = new UnitReloadCommand(
                        Context.selectedUnit,
                        1,
                        Context.EventBus
                    );
                    Context.CommandQueue.EnqueueAndExecute(reloadCommand);
                    break;
                case EActionType.SwitchWeapon:
                    var switchWeaponCommand = new UnitSwitchWeaponCommand(
                        Context.selectedUnit,
                        1,
                        Context.EventBus
                    );
                    Context.CommandQueue.EnqueueAndExecute(switchWeaponCommand);
                    break;
				case EActionType.None:
				case EActionType.Interact:
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
			DeselectUnit();
			StateMachine(Context).ChangeState<IdleState>();
		}

		private void OnEsc(EscInputEvent e)
		{
			this.Log("Esc input received - Deselecting unit and going idle");
			DeselectUnit();
			StateMachine(Context).ChangeState<IdleState>();
		}

		private void OnPointerHover(PointerHoverEvent e) => PublishBasicCursorInfo(Context, e);

		private void SwitchToUnit(Unit.Unit newUnit)
		{
			this.Log($"Switching to unit: {newUnit.name}");

            DeselectUnit();

			// Update selection
			Context.selectedUnit = newUnit;

			// Publish new selection
			Publish(Context, new UnitSelectedEvent(
				newUnit.id,
				newUnit.position
			));
		}

		private void ExecuteWait()
		{
			this.Log("Executing Wait action");
			DeselectUnit();
			Context.TurnService.EndUnitTurn(); // => 发出 UnitTurnEndedEvent,GameServer 监听 UnitTurnEndedEvent 会接管后续逻辑
		}
        
        private void DeselectUnit()
        {
	        Publish(Context, new UnitDeselectedEvent(Context.selectedUnit.id));
	        Context.ClearSelection();
        }
	}
}
