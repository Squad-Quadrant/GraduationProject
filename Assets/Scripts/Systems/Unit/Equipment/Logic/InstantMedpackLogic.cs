using Core.Commands;
using Core.Log;
using Data.Runtime.Commands;
using Systems.Interaction;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment.Config;

namespace Systems.Unit.Equipment.Logic
{
	public class InstantMedpackLogic : TacticalItemLogic, IInstantUsable
	{
		public InstantMedpackLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public ICommand CreateCommand(InteractionContext ctx) =>
			new UseInstantItemCommand(this, ItemConfig.apCost, ItemConfig.healAmount);
	}
}
