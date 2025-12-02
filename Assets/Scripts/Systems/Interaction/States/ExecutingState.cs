using System;
using Core.Commands.Events;
using Data.Runtime.Events.Interaction;

namespace Systems.Interaction.States
{
	public class ExecutingState : InteractionState
	{
		private Action<CommandCompletedEvent> _onQueueCompleted;

		public ExecutingState() : base(InteractionStates.Executing) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			Log("[ExecutingState] Entered - waiting for commands to complete");

			// Check if queue is already empty (shouldn't happen, but safety check)
			if (ctx.CommandQueue.IsIdle)
			{
				Log("[ExecutingState] Queue already idle, transitioning immediately");
				DetermineNextState();
				return;
			}

			_onQueueCompleted = OnCommandsCompleted;
			Subscribe(ctx, _onQueueCompleted);
		}

		public override void OnExit(InteractionContext ctx)
		{
			Log("[ExecutingState] Exited");

			Unsubscribe(ctx, _onQueueCompleted);
			_onQueueCompleted = null;

			base.OnExit(ctx);
		}

		public override void OnUpdate(InteractionContext ctx, float deltaTime)
		{
			base.OnUpdate(ctx, deltaTime);

			// todo: could add a timeout here to avoid infinite waiting
		}

		private void OnCommandsCompleted(CommandCompletedEvent commandCompletedEvent)
		{
			Log("[ExecutingState] All commands completed");
			DetermineNextState();
		}

		private void DetermineNextState()
		{
			// Check if we still have a selected unit
			if (Context.selectedUnit == null)
			{
				Log("[ExecutingState] No selected unit, going to Idle");
				Context.StateMachine.ChangeState<IdleState>();
				return;
			}

			var unit = Context.selectedUnit;
			var currentTurnUnit = Context.TurnService.GetCurrentUnit();

			// todo: should check action points
			if (currentTurnUnit != null && currentTurnUnit.Id == unit.id && currentTurnUnit.CanAct)
			{
				// Unit can still act - return to unit selected
				Log($"[ExecutingState] Unit {unit.name} can still act, returning to UnitSelected");

				// Re-publish selection event (UI might need to refresh)
				Publish(Context, new UnitSelectedEvent(
					unit.id,
					unit.position,
					Context.availableActions
				));

				Context.StateMachine.ChangeState(new UnitSelectedState());
			}
			else
			{
				// Unit's turn is over
				Log($"[ExecutingState] Unit {unit.name} turn complete, going to Idle");

				Context.TurnService.EndTurn();

				Context.StateMachine.ChangeState(new IdleState());
			}
		}
	}
}
