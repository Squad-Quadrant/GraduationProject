using Core.Events;

namespace Data.Runtime.Events.BattleFlow
{
	public readonly struct BattleOverEvent : IEvent
	{
		public bool IsVictory { get; }

		public BattleOverEvent(bool isVictory) => IsVictory = isVictory;

		public override string ToString() => $"[GameOver] {(IsVictory ? "Victory" : "Defeat")}";
	}
}
