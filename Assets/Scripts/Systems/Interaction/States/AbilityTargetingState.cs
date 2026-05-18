using System;
using System.Collections.Generic;
using System.Linq;
using Core.Commands;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Config;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Systems.Interaction.States
{
	public class AbilityTargetingState : InteractionState
	{
		private Action<PointerHoverEvent> _onPointerHover;
		private Action<CellClickedEvent> _onCellClicked;
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;

		private ITargeted _ability;
		private IReadOnlyList<Vector2Int> _validCells;
		private Vector2Int? _lastAoEHoverCell;

		public AbilityTargetingState() : base(InteractionStates.AbilityTargeting) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			if (ctx.selectedUnit == null)
				throw new InvalidOperationException("No unit selected when entering AbilityTargetingState.");

			_ability = ctx.PendingAbility ?? throw new InvalidOperationException("No pending ability when entering AbilityTargetingState.");
			_validCells = _ability.GetValidCells(ctx) ?? Array.Empty<Vector2Int>();

			this.Log($"Entered - Unit: {ctx.selectedUnit.name}, Ability: {_ability.GetType().Name}, ValidCells: {_validCells.Count}");

			Publish(ctx, new RangeDisplayEvent(
				ERangeType.Interact,
				_validCells,
				origin: ctx.selectedUnit.position,
				sourceUnitId: ctx.selectedUnit.id));

			_onPointerHover = OnPointerHover;
			_onCellClicked = OnCellClicked;
			_onUnitClicked = OnUnitClicked;
			_onBack = OnBack;
			_onEsc = OnEsc;

			Subscribe(ctx, _onPointerHover);
			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Interact));
			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.AreaEffectOverlay));
			Publish(ctx, CursorInfoEvent.Hide());

			Unsubscribe(ctx, _onPointerHover);
			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);

			_onPointerHover = null;
			_onCellClicked = null;
			_onUnitClicked = null;
			_onBack = null;
			_onEsc = null;

			_ability = null;
			_validCells = null;
			_lastAoEHoverCell = null;

			ctx.PendingAbility = null;

			base.OnExit(ctx);
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			PublishBasicCursorInfo(Context, e);

			Vector2Int? hoverCell = null;
			if (!string.IsNullOrEmpty(e.HoveredUnitId) &&
			    Context.UnitService.TryGetUnit(e.HoveredUnitId, out var hoverUnit))
				hoverCell = hoverUnit.position;
			else if (e.CellPosition.HasValue)
				hoverCell = e.CellPosition.Value;

			if (!hoverCell.HasValue || !_validCells.Contains(hoverCell.Value))
			{
				Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
				_lastAoEHoverCell = null;
				return;
			}

			if (_lastAoEHoverCell == hoverCell) return;
			_lastAoEHoverCell = hoverCell;

			var aoeCells = _ability.GetAreaEffectPreview(hoverCell.Value);
			if (aoeCells == null || aoeCells.Count == 0)
			{
				Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
				return;
			}

			Publish(Context, new RangeDisplayEvent(
				ERangeType.AreaEffectPreview, aoeCells,
				origin: hoverCell, sourceUnitId: Context.selectedUnit.id,
				areaEffectColor: Color.red));
		}

		private void OnCellClicked(CellClickedEvent e) => TryExecuteAt(e.CellPosition);

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (!Context.UnitService.TryGetUnit(e.UnitId, out var target))
			{
				this.Log($"Unit not found: {e.UnitId}");
				return;
			}
			TryExecuteAt(target.position);
		}

		private void TryExecuteAt(Vector2Int cell)
		{
			if (!_validCells.Contains(cell))
			{
				this.Log($"Cell {cell} not in valid cells, ignored");
				return;
			}

			if (!_ability.ValidateTarget(cell, Context))
			{
				this.Log($"Cell {cell} failed ValidateTarget, ignored");
				return;
			}

			this.Log($"Executing ability at {cell}");
			var cmd = _ability.CreateCommand(cell, Context);
			Context.CommandQueue.EnqueueAndExecute(cmd);
			StateMachine(Context).ChangeState<ExecutingState>();
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
	}
}
