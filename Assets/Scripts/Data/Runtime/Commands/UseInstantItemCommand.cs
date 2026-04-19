using Core.Commands;
using Core.Log;
using DG.Tweening;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Data.Runtime.Commands
{
	public class UseInstantItemCommand : AsyncCommand
	{
		private const float FeedbackDuration = 0.2f;

		private readonly TacticalItemLogic _logic;
		private readonly int _apCost;
		private readonly int _healAmount;

		public override string Name => $"UseInstant({_logic.Owner.name} +{_healAmount}HP)";
		public override bool CanUndo => false;

		public UseInstantItemCommand(TacticalItemLogic logic, int apCost, int healAmount)
		{
			_logic = logic;
			_apCost = apCost;
			_healAmount = healAmount;
		}

		protected override void OnExecuteAsync()
		{
			this.Log($"Executing: {Name}");

			var owner = _logic.Owner;

			owner.CurrentAp -= _apCost;
			_logic.Consume();

			int newHp = Mathf.Min(owner.CurrentHp + _healAmount, owner.maxHp);
			int actualHeal = newHp - owner.CurrentHp;
			owner.CurrentHp = newHp;

			this.Log($"{owner.name} healed {actualHeal} HP → {owner.CurrentHp}/{owner.maxHp}");

			DOVirtual.DelayedCall(FeedbackDuration, CompleteExecution);
		}
	}
}
