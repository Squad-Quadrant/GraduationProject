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

			if (ctx.selectedUnit == null)
				throw new InvalidOperationException("Entered UnitSelectedState without a selected unit.");

			this.Log($"Entered - Unit: {ctx.selectedUnit.name}");

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
                        0,
                        Context.EventBus
                    );
                    Context.CommandQueue.EnqueueAndExecute(switchWeaponCommand);
                    break;

				case EActionType.Interact:
					StateMachine(Context).ChangeState<InteractPreviewState>();
					break;

				case EActionType.UseTacticalItem:
					StateMachine(Context).ChangeState<ItemSelectionState>();
					break;
				case EActionType.None:
				case EActionType.Defend:
				case EActionType.Count:
				default:
					this.LogWarning($"Unhandled action: {e.ActionType}");
					break;
			}
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{

		}

		private void OnBack(BackInputEvent e)
		{

		}

		private void OnEsc(EscInputEvent e)
		{

		}

		private void OnPointerHover(PointerHoverEvent e) => PublishBasicCursorInfo(Context, e);

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
