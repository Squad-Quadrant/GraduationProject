using Core.Events;
using Systems.Unit;

namespace Data.Runtime.Events.Turn
{
	/// <summary>
	/// Triggered when:
	///  - A unit's turn starts.
	///  - Call NextUnit() manually.
	/// </summary>
	public readonly struct UnitTurnStartedEvent : IEvent
	{
		public string UnitId { get; }

		public int TurnNumber { get; }

		public bool IsVisibleToPlayer { get; } // 现在这个单位在不在友方视野里

		public UnitTurnStartedEvent(string unitId, int turnNumber, bool isVisibleToPlayer)
		{
			UnitId = unitId;
			TurnNumber = turnNumber;
			IsVisibleToPlayer = isVisibleToPlayer;
		}

		public override string ToString() => $"[UnitTurnStarted] Unit '{UnitId}' on Turn {TurnNumber}";
	}
}
