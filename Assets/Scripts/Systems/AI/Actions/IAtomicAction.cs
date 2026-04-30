using Core.Commands;

namespace Systems.AI.Actions
{
	// Plan 序列里的"动作意图
	public interface IAtomicAction
	{
		ICommand CreateCommand(AIContext ctx);
	}
}
