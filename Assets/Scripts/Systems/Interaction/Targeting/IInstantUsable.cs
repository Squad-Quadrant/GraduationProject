using Core.Commands;

namespace Systems.Interaction.Targeting
{
	// 即时生效的道具/技能
	// 进入 ItemSelectionState 选中此类 Logic 后，直接构造 Command 执行，不进入 TargetingState
	public interface IInstantUsable
	{
		ICommand CreateCommand(InteractionContext ctx);
	}
}
