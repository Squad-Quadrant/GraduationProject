using Core.FSM;
using UnityEngine;

namespace Test.WZHTest.FSM.States
{
	public class PlayerTurnState : IState<TurnContext>
	{
		public string Name => "PlayerTurn";

		public void OnEnter(TurnContext ctx)
		{
			Debug.Log($"===== 玩家回合开始 (第 {ctx.turnNumber} 回合) =====");
			ctx.isPlayerTurn = true;
			ctx.turnTimer = 0f;
		}

		public void OnUpdate(TurnContext ctx, float deltaTime)
		{
			ctx.turnTimer += deltaTime;
		}

		public void OnExit(TurnContext ctx)
		{
			Debug.Log("玩家回合结束");
			ctx.isPlayerTurn = false;
			ctx.turnNumber++;
		}
	}
}
