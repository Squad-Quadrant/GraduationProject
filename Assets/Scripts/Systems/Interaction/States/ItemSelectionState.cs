using System;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.UI;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment;

namespace Systems.Interaction.States
{
	public class ItemSelectionState : InteractionState
	{
		private Action<TacticalItemSelectedEvent> _onTacticalItemSelected;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;
		private Action<PointerHoverEvent> _onPointerHover;

		public ItemSelectionState() : base(InteractionStates.ItemSelection) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			this.Log($"Entered - Unit: {ctx.selectedUnit?.name}");

			if (ctx.selectedUnit == null)
			{
				this.LogError("No unit selected! Returning to Idle.");
				ctx.StateMachine.ChangeState<IdleState>();
				return;
			}

			_onTacticalItemSelected = OnTacticalItemSelected;
			_onBack = OnBack;
			_onEsc = OnEsc;
			_onPointerHover = OnPointerHover;

			Subscribe(ctx, _onTacticalItemSelected);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
			Subscribe(ctx, _onPointerHover);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Unsubscribe(ctx, _onTacticalItemSelected);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);
			Unsubscribe(ctx, _onPointerHover);

			_onTacticalItemSelected = null;
			_onBack = null;
			_onEsc = null;
			_onPointerHover = null;

			base.OnExit(ctx);
		}

		private void OnTacticalItemSelected(TacticalItemSelectedEvent e)
		{
			var unit = Context.selectedUnit;
			var item = unit.GetTacticalItem(e.SlotIndex);

			if (item.IsNullOrEmpty())
			{
				this.LogWarning($"Selected slot {e.SlotIndex} is empty. Ignoring.");
				return;
			}

			var logic = item.Logic;

			switch (item.Logic)
			{
				case IInstantUsable instantUsable:
					this.Log($"Slot {e.SlotIndex} is IInstantUsable. Creating command.");
					var cmd = instantUsable.CreateCommand(Context);
					Context.CommandQueue.EnqueueAndExecute(cmd);
					StateMachine(Context).ChangeState<ExecutingState>();
					break;

				case ITargeted targeted:
					this.Log($"Slot {e.SlotIndex} is ITargeted. Entering TargetingState.");
					Context.PendingTargeting = targeted;
					StateMachine(Context).ChangeState<TargetingState>();
					break;

				default:
					this.LogWarning($"Slot {e.SlotIndex} Logic '{logic?.GetType().Name ?? "<null>"}' implements neither IInstantUsable nor ITargeted");
					break;
			}
		}

		private void OnBack(BackInputEvent e)
		{
			this.Log("Back → UnitSelected");
			StateMachine(Context).ChangeState<UnitSelectedState>();
		}

		private void OnEsc(EscInputEvent e)
		{
			this.Log("Esc → Idle");
			StateMachine(Context).ChangeState<IdleState>();
		}

		private void OnPointerHover(PointerHoverEvent e) => PublishBasicCursorInfo(Context, e);
	}
}
