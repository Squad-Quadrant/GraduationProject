using System;

namespace Systems.Turn
{
	[Serializable]
	public class TurnData
	{
		public int CurrentTurnNumber { get; set; } = 0;

		public bool IsTurnActive { get; set; } = false;

		public ITurnUnit CurrentActingUnit { get; set; } = null;

		public bool HasActingUnit => CurrentActingUnit != null;

		public TurnQueue ActionQueue { get; private set; } = new();

		public void Reset()
		{
			CurrentTurnNumber = 0;
			IsTurnActive = false;
			CurrentActingUnit = null;
			ActionQueue.Clear();
		}

		public override string ToString()
		{
			var actingUnit = CurrentActingUnit != null ? CurrentActingUnit.Id : "None";
			return $"[TurnData] Turn:{CurrentTurnNumber}, Active:{IsTurnActive}, " +
			       $"Acting:{actingUnit}, QueueSize:{ActionQueue.Count}";
		}
	}
}
