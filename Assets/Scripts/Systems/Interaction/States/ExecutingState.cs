using System;
using Core.Commands.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Vision;

namespace Systems.Interaction.States
{
	public class ExecutingState : InteractionState
	{
		private Action<CommandCompletedEvent> _onQueueCompleted;

		public ExecutingState() : base(InteractionStates.Executing) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			this.Log("Entered - waiting for commands to complete");

			// Check if queue is already empty (shouldn't happen, but safety check)
			if (ctx.CommandQueue.IsIdle)
			{
				this.Log("Queue already idle, transitioning immediately");
				DetermineNextState();
				return;
			}

			_onQueueCompleted = OnCommandsCompleted;
			Subscribe(ctx, _onQueueCompleted);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			if (_onQueueCompleted != null)
			{
				Unsubscribe(ctx, _onQueueCompleted);
				_onQueueCompleted = null;
			}

			base.OnExit(ctx);
		}

		public override void OnUpdate(InteractionContext ctx, float deltaTime)
		{
			base.OnUpdate(ctx, deltaTime);

			// todo: could add a timeout here to avoid infinite waiting
		}

		private void OnCommandsCompleted(CommandCompletedEvent commandCompletedEvent)
		{
			this.Log("All commands completed");
			DetermineNextState();
		}

		private void DetermineNextState()
		{
			var simResult = Context.LastSimulationResult;
			if (simResult is { WasInterrupted: true })
			{
				this.Log($"Movement was interrupted — {simResult.DiscoveredUnits.Count} enemies discovered");

				Publish(Context, new EnemiesDiscoveredEvent(Context.selectedUnit?.id, simResult.DiscoveredUnits));

				Context.VisibleCells = simResult.FinalVisibleCells;
				Publish(Context, new VisionChangedEvent(Context.VisibleCells, Context.selectedUnit?.id));
			}
			Context.LastSimulationResult = null;

			// Check if we still have a selected unit
			if (Context.selectedUnit == null)
			{
				this.Log("No selected unit");
				Context.TurnService.EndUnitTurn();
				return;
			}
            
			var unit = Context.selectedUnit;
			var currentTurnUnit = Context.TurnService.ActiveUnit;

			if (currentTurnUnit != null && currentTurnUnit.Id == unit.id && currentTurnUnit.CanAct)
			{
				// Unit can still act - return to unit selected
				this.Log($"Unit {unit.name} still acting (AP:{unit.currentAp}), returning to UnitSelected");

				// Re-publish selection event (UI might need to refresh)
				Publish(Context, new UnitSelectedEvent(
					unit.id,
					unit.position
				));

				Context.StateMachine.ChangeState<UnitSelectedState>();
			}
			else
			{
				// Unit's turn is over
				this.Log($"Unit {unit.name} turn complete");
				Context.TurnService.EndUnitTurn();
			}
		}
	}
}
