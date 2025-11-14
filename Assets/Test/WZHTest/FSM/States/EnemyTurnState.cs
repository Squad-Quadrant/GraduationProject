using Core.FSM;
using UnityEngine;

namespace Test.WZHTest.FSM.States
{
	public class EnemyTurnState : IState<TurnContext>
	{
		public string Name => "EnemyTurn";

		public void OnEnter(TurnContext context)
		{
			Debug.Log($"===== 敌人回合开始 =====");
			context.isPlayerTurn = false;
			context.turnTimer = 0f;
		}

		public void OnUpdate(TurnContext context, float deltaTime)
		{
			context.turnTimer += deltaTime;
		}

		public void OnExit(TurnContext context)
		{
			Debug.Log("敌人回合结束");
			context.turnNumber++;
		}
	}
}
