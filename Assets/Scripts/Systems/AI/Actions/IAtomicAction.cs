using Core.Commands;
using Data.Runtime;

namespace Systems.AI.Actions
{
	// Plan 序列里的"动作意图
	public interface IAtomicAction
	{
		EActionType ActionType { get; }

		ICommand CreateCommand(AIContext ctx);
	}
}
