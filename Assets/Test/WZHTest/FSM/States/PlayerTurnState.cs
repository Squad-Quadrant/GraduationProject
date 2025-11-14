using Core.FSM;
using UnityEngine;

namespace Test.WZHTest.FSM.States
{
	public class PlayerTurnState : IState<TurnContext>
	{
		public string Name => "PlayerTurn";

		public void OnEnter(TurnContext context)
		{
			Debug.Log($"===== 玩家回合开始 (第 {context.turnNumber} 回合) =====");
			context.isPlayerTurn = true;
			context.turnTimer = 0f;
		}

		public void OnUpdate(TurnContext context, float deltaTime)
		{
			context.turnTimer += deltaTime;
		}

		public void OnExit(TurnContext context)
		{
			Debug.Log("玩家回合结束");
			context.isPlayerTurn = false;
			context.turnNumber++;
		}
	}
}
