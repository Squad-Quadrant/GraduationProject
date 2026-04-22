using System;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.UI;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment;

namespace Systems.Interaction.States
{
	public class AbilitySelectionState : InteractionState
	{
		private Action<TacticalItemSelectedEvent> _onTacticalItemSelected;
		private Action<SkillSelectedEvent> _onSkillSelected;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;
		private Action<PointerHoverEvent> _onPointerHover;

		public AbilitySelectionState() : base(InteractionStates.AbilitySelection) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			if (ctx.selectedUnit == null)
				throw new InvalidOperationException("No unit selected when entering ItemSelectionState.");

			this.Log($"Entered - Unit: {ctx.selectedUnit.name}");

			_onTacticalItemSelected = OnTacticalItemSelected;
			_onSkillSelected = OnSkillSelected;
			_onBack = OnBack;
			_onEsc = OnEsc;
			_onPointerHover = OnPointerHover;

			Subscribe(ctx, _onTacticalItemSelected);
			Subscribe(ctx, _onSkillSelected);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
			Subscribe(ctx, _onPointerHover);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Unsubscribe(ctx, _onTacticalItemSelected);
			Unsubscribe(ctx, _onSkillSelected);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);
			Unsubscribe(ctx, _onPointerHover);

			_onTacticalItemSelected = null;
			_onSkillSelected = null;
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

			this.Log($"TacticalItem slot {e.SlotIndex} selected.");
			DispatchLogic(item.Logic);
		}

		private void OnSkillSelected(SkillSelectedEvent e)
		{
			var unit = Context.selectedUnit;
			if (unit.Skill == null)
			{
				this.LogWarning("SkillSelectedEvent received but unit has no skill. Ignoring.");
				return;
			}

			this.Log("Skill selected.");
			DispatchLogic(unit.Skill);
		}

		private void DispatchLogic(object logic)
		{
			switch (logic)
			{
				case IInstantUsable instantUsable:
					this.Log("Logic is IInstantUsable → creating command and entering ExecutingState.");
					var cmd = instantUsable.CreateCommand(Context);
					Context.CommandQueue.EnqueueAndExecute(cmd);
					StateMachine(Context).ChangeState<ExecutingState>();
					break;

				case ITargeted targeted:
					this.Log("Logic is ITargeted → entering TargetingState.");
					Context.PendingTarget = targeted;
					StateMachine(Context).ChangeState<TargetingState>();
					break;

				default:
					this.LogWarning($"Logic '{logic?.GetType().Name ?? "<null>"}' implements neither IInstantUsable nor ITargeted. Ignored.");
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
			this.Log("Esc → UnitSelected");
			StateMachine(Context).ChangeState<UnitSelectedState>();
		}

		private void OnPointerHover(PointerHoverEvent e) => PublishBasicCursorInfo(Context, e);
	}
}
