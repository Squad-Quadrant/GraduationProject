using System.Collections.Generic;
using Core.Commands;
using Core.Log;
using DG.Tweening;
using Systems.AreaEffect;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Data.Runtime.Commands
{
	public class ThrowAreaEffectCommand : AsyncCommand
	{
		private const float ThrowDuration = 0.5f;

		private readonly TacticalItemLogic _logic;
		private readonly int _apCost;
		private readonly Vector2Int _targetCell;
		private readonly IReadOnlyList<Vector2Int> _cells;
		private readonly int _persistTurns;
		private readonly AreaEffectBehavior _behavior;
		private readonly IAreaEffectService _areaEffectService;

		public override string Name => $"ThrowAreaEffect({_logic.Owner.name} → {_targetCell}, {_behavior.DisplayName}, persist={_persistTurns})";
		public override bool CanUndo => false;

		public ThrowAreaEffectCommand(
			TacticalItemLogic logic,
			int apCost,
			Vector2Int targetCell,
			IReadOnlyList<Vector2Int> cells,
			int persistTurns,
			AreaEffectBehavior behavior,
			IAreaEffectService areaEffectService)
		{
			_logic = logic;
			_apCost = apCost;
			_targetCell = targetCell;
			_cells = cells;
			_persistTurns = persistTurns;
			_behavior = behavior;
			_areaEffectService = areaEffectService;
		}

		protected override void OnExecuteAsync()
		{
			this.Log($"Executing: {Name}");

			_logic.Owner.CurrentAp -= _apCost;
			_logic.Consume();

			DOVirtual.DelayedCall(ThrowDuration, OnLanded);
		}

		private void OnLanded()
		{
			var effect = _areaEffectService.Register(
				ownerId:       _logic.Owner.id,
				targetCell:    _targetCell,
				cells:         _cells,
				remainingTurns: _persistTurns,
				behavior:      _behavior);

			this.Log($"Registered {effect}");

			CompleteExecution();
		}
	}
}
