using System;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Commands;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment;

namespace Systems.Interaction.States
{
	public class UnitSelectedState : InteractionState
	{
		private Action<ActionSelectedEvent> _onActionSelected;
		private Action<UnitClickedEvent> _onUnitClicked;
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
			_onPointerHover = OnPointerHover;
            
			Subscribe(ctx, _onActionSelected);
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onPointerHover);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Unsubscribe(ctx, _onActionSelected);
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onPointerHover);

			_onActionSelected = null;
			_onUnitClicked = null;
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
					bool precise = e.Payload == 1;
					var weapon = Context.selectedUnit.CurrentWeaponLogic;
					if (weapon == null)
					{
						this.LogError("Attack selected but no weapon equipped");
						return;
					}
					weapon.IsOnPreciseShoot = precise;
					StateMachine(Context).ChangeState<AttackPreviewState>();
					break;

                case EActionType.Reload:
                    var reloadCommand = new UnitReloadCommand(
                        Context.selectedUnit,
                        1,
                        Context.EventBus,
                        Context.AudioService
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
					int slotIndex = e.Payload;
					var container = Context.selectedUnit.GetTacticalItem(slotIndex);
					if (container.IsNullOrEmpty())
					{
						this.LogError($"Tactical item slot {slotIndex} is empty");
						return;
					}
					if (container.Logic is not ITargeted tacticalItemLogic)
					{
						this.LogError($"Tactical item logic at slot {slotIndex} does not implement ITargeted: {container.Logic?.GetType().Name ?? "null"}.");
						return;
					}
					Context.PendingAbility = tacticalItemLogic;
					StateMachine(Context).ChangeState<AbilityTargetingState>();
					break;

				case EActionType.UseSkill:
					var skill = Context.selectedUnit.Skill;
					if (skill == null)
					{
						this.LogError("UseSkill selected but unit has no skill");
						return;
					}
					if (skill is not ITargeted skillLogic)
					{
						this.LogError(
							$"Skill logic {skill.GetType().Name} does not implement ITargeted.");
						return;
					}
					Context.PendingAbility = skillLogic;
					StateMachine(Context).ChangeState<AbilityTargetingState>();
					break;

				case EActionType.Wait:
					this.Log("Executing Wait action");
					DeselectUnit();
					Context.TurnService.EndUnitTurn(); // => 发出 UnitTurnEndedEvent,GameServer 监听 UnitTurnEndedEvent 会接管后续逻辑
					break;

				case EActionType.None:
				case EActionType.Defend:
				case EActionType.Count:
				case EActionType.Back:
				case EActionType.AI:
				default:
					this.LogWarning($"Unhandled action: {e.ActionType}");
					break;
			}
		}

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (!Context.UnitService.TryGetUnit(e.UnitId, out var unit))
			{
				this.LogWarning($"Clicked unit '{e.UnitId}' not found.");
				return;
			}

			if (unit.id == Context.selectedUnit.id) return;

			Context.inspectedUnit = unit;
			StateMachine(Context).ChangeState<UnitInspectState>();
		}

		private void OnPointerHover(PointerHoverEvent e) => PublishBasicCursorInfo(Context, e);
        
        private void DeselectUnit()
        {
	        Publish(Context, new UnitDeselectedEvent(Context.selectedUnit.id));
	        Context.Clear();
        }
	}
}
