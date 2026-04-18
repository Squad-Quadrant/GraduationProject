using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Systems.Map.SceneActor;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class InteractPreviewState : InteractionState
	{
		public InteractPreviewState() : base(InteractionStates.InteractPreview) { }

		private Action<CellClickedEvent> _onCellClicked;
		private Action<UnitClickedEvent> _onUnitClicked;

		private IReadOnlyList<Vector2Int> _validTargetCells;

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

			_validTargetCells = CalculateInteractableTargets(ctx);

			Publish(ctx, new RangeDisplayEvent(
				ERangeType.Interact,
				_validTargetCells,
				origin: ctx.selectedUnit.position,
				sourceUnitId: ctx.selectedUnit.id));

			_onCellClicked = OnCellClicked;
			_onUnitClicked = OnUnitClicked;
			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onUnitClicked);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Interact));

			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onUnitClicked);
			_onCellClicked = null;
			_onUnitClicked = null;

			base.OnExit(ctx);
		}

		private void OnCellClicked(CellClickedEvent e) => ConfirmInteraction(e.CellPosition);

		private void OnUnitClicked(UnitClickedEvent e) => ConfirmInteraction(e.CellPosition);

		private void ConfirmInteraction(Vector2Int cellPosition)
		{
			if (!_validTargetCells.Contains(cellPosition))
			{
				this.LogWarning($"Clicked cell {cellPosition} is not a valid interact target.");
				return;
			}

			this.Log($"Interacting with cell {cellPosition}");

			var actor = Context.MapService.Data.GetCell(cellPosition).SceneActor;
			if (actor is not InteractableSceneActor interactableActor)
			{
				this.LogError($"No interactable actor found at {cellPosition}!");
				return;
			}

			var selectedUnit = Context.selectedUnit;
			var interactCommand = new InteractCommand(selectedUnit, interactableActor, Context.EventBus);

			Context.CommandQueue.EnqueueAndExecute(interactCommand);
			Context.StateMachine.ChangeState<ExecutingState>();
		}

		private static List<Vector2Int> CalculateInteractableTargets(InteractionContext ctx)
		{
			var validTargetCells = new List<Vector2Int>();
			var neighbors = ctx.MapService.Data.GetNeighborCells(ctx.selectedUnit.position);
			neighbors.Add(ctx.MapService.Data.GetCell(ctx.selectedUnit.position)); // Include the unit's own cell for self-interaction

			foreach (var actor in neighbors.Select(neighbor => neighbor.SceneActor))
			{
				if (actor is not InteractableSceneActor interactableActor) continue;
				validTargetCells.Add(interactableActor.BaseCell.Position);
			}

			return validTargetCells;
		}
	}
}
