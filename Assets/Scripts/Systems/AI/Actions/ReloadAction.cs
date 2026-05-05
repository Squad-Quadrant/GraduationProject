using Core.Commands;
using Data.Runtime.Commands;

namespace Systems.AI.Actions
{
	public sealed class ReloadAction : IAtomicAction
	{
		public ICommand CreateCommand(AIContext ctx) => new UnitReloadCommand(ctx.Self, 1, ctx.EventBus, ctx.AudioService);

		public override string ToString() => "Reload";
	}
}
