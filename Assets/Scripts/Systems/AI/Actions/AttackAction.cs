using Core.Commands;
using Data.Runtime;
using Data.Runtime.Commands;

namespace Systems.AI.Actions
{
	public class AttackAction : IAtomicAction
	{
		public string TargetUnitId { get; }

		public AttackAction(string targetUnitId) => TargetUnitId = targetUnitId;

		public ICommand CreateCommand(AIContext ctx)
		{
			var unit = ctx.Self;
			return new UnitAttackCommand(
				unit.id,
				TargetUnitId,
				1,
				EActionType.Attack,
				ctx.UnitService,
				ctx.MapService,
				ctx.EventBus);
		}

		public override string ToString() => $"Attack→{TargetUnitId}";
	}
}
