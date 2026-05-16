using System;
using System.Collections.Generic;

namespace Systems.Turn
{
	[Serializable]
	public class TurnData
	{
		public int TurnNumber { get; set; }

		public bool IsTurnActive { get; set; } = false;

		public bool IsUnitActing { get; set; }

		public TurnQueue Queue { get; } = new();

		public Dictionary<string, ITurnUnit> RegisteredUnits { get; } = new();

		public ITurnUnit ActiveUnit => IsUnitActing ? Queue.CurrentUnit : null;

		public void Reset()
		{
			TurnNumber = 0;
			IsTurnActive = false;
			IsUnitActing = false;
			Queue.Clear();
			RegisteredUnits.Clear();
		}

		public override string ToString()
		{
			var acting = ActiveUnit?.Id ?? "None";
			return $"[TurnData] Turn:{TurnNumber}, Active:{IsTurnActive}, " +
			       $"Acting:{acting}, Queue:{Queue}";
		}
	}
}
