using Core.Events;

namespace Data.Runtime.Events.Vision
{
	public readonly struct EnemySpotClearedEvent : IEvent
	{
		public readonly string UnitId;

		public EnemySpotClearedEvent(string unitId) => UnitId = unitId;

		public override string ToString() => $"[EnemySpotCleared] Unit '{UnitId}'";
	}
}
