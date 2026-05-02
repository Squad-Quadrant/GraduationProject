using Systems.Turn;

namespace Systems.BattleFlow
{
	// GameServer内部接口，用于分离逻辑，简化逻辑
	public interface ITurnController
	{
		void BeginTurn(ITurnUnit turnUnit);
	}
}
