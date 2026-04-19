using System.Collections.Generic;
using Core.Commands;
using Core.Log;
using DG.Tweening;
using Systems.Unit;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Data.Runtime.Commands
{
	public class ThrowInstantAoECommand : AsyncCommand
	{
		private const float ThrowDuration = 0.5f;

		private readonly TacticalItemLogic _logic;
		private readonly int _apCost;
		private readonly Vector2Int _targetCell;
		private readonly IReadOnlyList<Vector2Int> _aoeCells;
		private readonly int _damage;
		private readonly IUnitService _unitService;

		public override string Name => $"ThrowAoE({_logic.Owner.name} → {_targetCell}, dmg={_damage}, cells={_aoeCells.Count})";
		public override bool CanUndo => false;

		public ThrowInstantAoECommand(
			TacticalItemLogic logic,
			int apCost,
			Vector2Int targetCell,
			IReadOnlyList<Vector2Int> aoeCells,
			int damage,
			IUnitService unitService)
		{
			_logic = logic;
			_apCost = apCost;
			_targetCell = targetCell;
			_aoeCells = aoeCells;
			_damage = damage;
			_unitService = unitService;
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
			foreach (var cell in _aoeCells)
			{
				var unit = _unitService.GetUnitAtPosition(cell);
				if (unit is not { IsAlive: true }) continue;

				// TODO(Damage): 等 IDamageService 扩展后替换
				this.Log($"[TODO(Damage)] Grenade @{cell}: would deal {_damage} to '{unit.name}'");
			}

			CompleteExecution();
		}
	}
}
