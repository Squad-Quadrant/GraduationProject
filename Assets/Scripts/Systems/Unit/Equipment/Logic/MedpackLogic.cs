using System.Collections.Generic;
using Core.Commands;
using DG.Tweening;
using Systems.Interaction;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
{
	public class MedpackLogic : TacticalItemLogic, ITargeted
	{
		public MedpackLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public IReadOnlyList<Vector2Int> GetValidCells(InteractionContext ctx) => new[] { Owner.position };

		public bool ValidateTarget(Vector2Int cell, InteractionContext ctx) => cell == Owner.position;

		public IReadOnlyList<Vector2Int> GetAreaEffectPreview(Vector2Int hoverCell) => new[] { hoverCell };

		public ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
			new AsyncLambdaCommand(
				$"{Owner.name} Use {Name()}",
				onComplete =>
				{
					Owner.CurrentAp -= ItemConfig.apCost;
					Consume();
					
					Owner.BuffProxy.Attach(ItemConfig.appliedBuff, this);

					foreach (var buffType in ItemConfig.otherBuffs)
						Owner.BuffProxy.Attach(buffType, this);
					
					DOVirtual.DelayedCall(0.2f, () => onComplete());
				});

        public override int GetDamage() => ItemConfig.directDamage;
	}
}
