using Core.Commands;
using Core.Log;
using DG.Tweening;
using Systems.Interaction;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
{
	public class InstantMedpackLogic : TacticalItemLogic, IInstantUsable
	{
		public InstantMedpackLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public ICommand CreateCommand(InteractionContext ctx) =>
			new AsyncLambdaCommand(
				$"{Owner.name} Use {Name()}",
				onComplete =>
				{
					Owner.CurrentAp -= ItemConfig.apCost;
					Consume();
					
					Owner.BuffProxy.Attach(ItemConfig.appliedBuff, this);
					
					DOVirtual.DelayedCall(0.2f, () => onComplete()); // todo: 需要动画或者反馈
				});

        public override int GetDamage()
        {
            return ItemConfig.directDamage;
        }
    }
}
