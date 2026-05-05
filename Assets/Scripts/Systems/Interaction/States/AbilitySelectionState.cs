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
using UnityEngine;

namespace Systems.Interaction.States
{
	public class AbilitySelectionState : InteractionState
	{
		private Action<TacticalItemSelectedEvent> _onTacticalItemSelected;
		private Action<SkillSelectedEvent> _onSkillSelected;
		private Action<PointerHoverEvent> _onPointerHover;
		private Action<CellClickedEvent> _onCellClicked;
		private Action<UnitClickedEvent> _onUnitClicked;
		private Action<BackInputEvent> _onBack;
		private Action<EscInputEvent> _onEsc;
		private Action<TargetConfirmEvent> _onTargetConfirm;
		private Action<ActionSelectedEvent> _onActionSelected;

		public AbilitySelectionState() : base(InteractionStates.AbilitySelection) { }

		private object _pendingLogic;

		// for ITarget only
		private IReadOnlyList<Vector2Int> _validCells;
		private Vector2Int? _targetCell;
		private Vector2Int? _lastAoEHoverCell;

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);

			if (ctx.selectedUnit == null)
				throw new InvalidOperationException("No unit selected when entering ItemSelectionState.");

			this.Log($"Entered - Unit: {ctx.selectedUnit.name}");

			_onTacticalItemSelected = OnTacticalItemSelected;
			_onSkillSelected = OnSkillSelected;
			_onPointerHover = OnPointerHover;
			_onCellClicked = OnCellClicked;
			_onUnitClicked = OnUnitClicked;
			_onBack = OnBack;
			_onEsc = OnEsc;
			_onTargetConfirm = OnTargetConfirm;
			_onActionSelected = OnActionSelected;

			Subscribe(ctx, _onTacticalItemSelected);
			Subscribe(ctx, _onSkillSelected);
			Subscribe(ctx, _onPointerHover);
			Subscribe(ctx, _onCellClicked);
			Subscribe(ctx, _onUnitClicked);
			Subscribe(ctx, _onBack);
			Subscribe(ctx, _onEsc);
			Subscribe(ctx, _onTargetConfirm);
			Subscribe(ctx, _onActionSelected);
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited");

			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.Interact));
			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
			Publish(ctx, RangeDisplayEvent.Clear(ERangeType.AreaEffectOverlay));
			Publish(ctx, TargetingEvent.Clear());
			Publish(ctx, CursorInfoEvent.Hide());

			Unsubscribe(ctx, _onTacticalItemSelected);
			Unsubscribe(ctx, _onSkillSelected);
			Unsubscribe(ctx, _onPointerHover);
			Unsubscribe(ctx, _onCellClicked);
			Unsubscribe(ctx, _onUnitClicked);
			Unsubscribe(ctx, _onBack);
			Unsubscribe(ctx, _onEsc);
			Unsubscribe(ctx, _onTargetConfirm);
			Unsubscribe(ctx, _onActionSelected);

			_onTacticalItemSelected = null;
			_onSkillSelected = null;
			_onPointerHover = null;
			_onCellClicked = null;
			_onUnitClicked = null;
			_onBack = null;
			_onEsc = null;
			_onTargetConfirm = null;
			_onActionSelected = null;

			_pendingLogic = null;
			_validCells = null;
			_targetCell = null;
			_lastAoEHoverCell = null;

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
					_pendingLogic = instantUsable;
					_validCells = Array.Empty<Vector2Int>();
					Publish(Context, RangeDisplayEvent.Clear(ERangeType.Interact));
					Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
					Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectOverlay));
					Publish(Context, new TargetingEvent(Context.selectedUnit.position)); // InstantUsable应该直接可确认
					_targetCell = null;
					break;

				case ITargeted targeted:
					_validCells = targeted.GetValidCells(Context) ?? Array.Empty<Vector2Int>();
					Publish(Context, new RangeDisplayEvent(ERangeType.Interact, _validCells, origin: Context.selectedUnit.position, sourceUnitId: Context.selectedUnit.id));
					if (_pendingLogic is not ITargeted || !_targetCell.HasValue || !_validCells.Contains(_targetCell.Value))
						Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectOverlay));
					_pendingLogic = targeted;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			PublishBasicCursorInfo(Context, e);

			if (_pendingLogic is not ITargeted pendingTargeted)
			{
				Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
				_lastAoEHoverCell = null;
				return;
			}

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

			var aoeCells = pendingTargeted.GetAreaEffectPreview(hoverCell.Value);
			if (aoeCells == null || aoeCells.Count == 0)
			{
				Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectPreview));
				return;
			}

			Publish(Context, new RangeDisplayEvent(ERangeType.AreaEffectPreview, aoeCells, origin: hoverCell, sourceUnitId: Context.selectedUnit.id, areaEffectColor: Color.red));
		}

		private void OnCellClicked(CellClickedEvent e) => Targeting(e.CellPosition);

		private void OnUnitClicked(UnitClickedEvent e)
		{
			if (!Context.UnitService.TryGetUnit(e.UnitId, out var target))
			{
				this.Log($"Unit not found: {e.UnitId}");
				return;
			}
			Targeting(target.position);
		}

		private void Targeting(Vector2Int cellPosition)
		{
			if (_pendingLogic == null) return;
			if (_validCells == null || !_validCells.Contains(cellPosition)) return;
			_targetCell = cellPosition;
			Publish(Context, new TargetingEvent(_targetCell));

			if (_pendingLogic is not ITargeted pendingTargeted)
			{
				Publish(Context, RangeDisplayEvent.Clear(ERangeType.AreaEffectOverlay));
				return;
			}
			Publish(Context, new RangeDisplayEvent(ERangeType.AreaEffectOverlay, pendingTargeted.GetAreaEffectPreview(_targetCell.Value)));
		}

		private void OnBack(BackInputEvent e)
		{
			if (_pendingLogic != null)
			{
				_pendingLogic = null;
				_validCells = null;
				_lastAoEHoverCell = null;
				Publish(Context, TargetingEvent.Clear());
				return;
			}

			this.Log("Back → UnitSelected");
			StateMachine(Context).ChangeState<UnitSelectedState>();
		}

		private void OnEsc(EscInputEvent e)
		{
			this.Log("Esc → UnitSelected");
			StateMachine(Context).ChangeState<UnitSelectedState>();
		}

		private void OnTargetConfirm(TargetConfirmEvent e)
		{
			if (_pendingLogic == null)
			{
				this.LogError("PendingLogic is null; Cannot execute Logic.");
				return;
			}

			ExecuteLogic(_pendingLogic);
		}

		private void ExecuteLogic(object logic)
		{
			if (!_targetCell.HasValue)
			{
				this.LogError($"Target cell null");
				return;
			}

			ICommand cmd = logic switch
			{
				IInstantUsable instantUsable => instantUsable.CreateCommand(Context),
				ITargeted targeted => targeted.CreateCommand(_targetCell.Value, Context),
				_ => throw new ArgumentOutOfRangeException()
			};

			Context.CommandQueue.EnqueueAndExecute(cmd);
			StateMachine(Context).ChangeState<ExecutingState>();
		}

		private void OnActionSelected(ActionSelectedEvent e)
		{
			if (e.ActionType != EActionType.Back) return;

			StateMachine(Context).ChangeState<UnitSelectedState>();
		}
	}
}
