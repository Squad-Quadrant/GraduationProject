using System;

namespace WZHTest.FSM
{
	[Serializable]
	public class TurnContext
	{
		public int turnNumber = 0;
		public bool isPlayerTurn;
		public bool allEnemiesDefeated = false;
		public bool playerDefeated = false;
		public float turnTimer;
		public float maxTurnTime = 5f;
	}
}
