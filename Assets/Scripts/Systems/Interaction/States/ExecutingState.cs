using System;
using System.Collections.Generic;
using Core.Commands.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.View;
using Data.Runtime.Events.Vision;
using Systems.Vision;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class ExecutingState : InteractionState
	{
		private Action<CommandCompletedEvent> _onQueueCompleted;
		private Action<PresentationCompleteEvent> _onDiscoveryFocusComplete;

		private RevealToken _revealToken = RevealToken.Invalid;

		public ExecutingState() : base(InteractionStates.Executing) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			this.Log("Entered - waiting for commands to complete");

			// Check if queue is already empty (shouldn't happen, but safety check)
			if (ctx.CommandQueue.IsIdle)
			{
				this.Log("Queue already idle, transitioning immediately");
				OnCommandsCompleted();
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

			CleanupDiscoverySubscription();

			if (_revealToken.IsValid)
			{
				Context.VisionService.RemoveTemporaryReveal(_revealToken);
				_revealToken = RevealToken.Invalid;
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
			OnCommandsCompleted();
		}

		private void OnCommandsCompleted()
		{
			var simResult = Context.LastSimulationResult;
			Context.LastSimulationResult = null;

			if (simResult is not { WasInterrupted: true })
			{
				ResolveNextState();
				return;
			}

			this.Log($"Movement interrupted — {simResult.DiscoveredUnits.Count} enemies discovered");

			// 打开临时视野，让玩家知道谁打断了他的移动
			var revealCells = new List<Vector2Int>(simResult.FinalVisibleCells);
			_revealToken = Context.VisionService.AddTemporaryReveal(revealCells);

			Publish(Context, new EnemiesDiscoveredEvent(Context.selectedUnit?.id, simResult.DiscoveredUnits));

			_onDiscoveryFocusComplete = OnDiscoveryFocusComplete;
			Subscribe(Context, _onDiscoveryFocusComplete);

			this.Log("Waiting for discovery focus to complete...");
		}

		private void OnDiscoveryFocusComplete(PresentationCompleteEvent e)
		{
			if (!e.Matches(EPresentationCategory.Camera, PresentationType.Camera.DiscoveryFocus))
				return;

			this.Log("Discovery focus complete, resuming");
			CleanupDiscoverySubscription();

			if (_revealToken.IsValid)
			{
				Context.VisionService.RemoveTemporaryReveal(_revealToken);
				_revealToken = RevealToken.Invalid;
			}

			ResolveNextState();
		}

		private void ResolveNextState()
		{
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
				this.Log($"Unit {unit.name} still acting (AP:{unit.CurrentAp}), returning to UnitSelected");

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

		private void CleanupDiscoverySubscription()
		{
			if (_onDiscoveryFocusComplete == null) return;
			Unsubscribe(Context, _onDiscoveryFocusComplete);
			_onDiscoveryFocusComplete = null;
		}
	}
}
