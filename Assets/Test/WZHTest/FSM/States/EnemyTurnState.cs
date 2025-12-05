using Core.FSM;
using UnityEngine;

namespace Test.WZHTest.FSM.States
{
	public class EnemyTurnState : IState<TurnContext>
	{
		public string Name => "EnemyTurn";

		public void OnEnter(TurnContext ctx)
		{
			Debug.Log($"===== 敌人回合开始 =====");
			ctx.isPlayerTurn = false;
			ctx.turnTimer = 0f;
		}

		public void OnUpdate(TurnContext ctx, float deltaTime)
		{
			ctx.turnTimer += deltaTime;
		}

		public void OnExit(TurnContext ctx)
		{
			Debug.Log("敌人回合结束");
			ctx.turnNumber++;
		}
	}
}
