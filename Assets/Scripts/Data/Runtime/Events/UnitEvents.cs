using Core.Events;
using Systems.Unit;
using UnityEngine;

namespace Data.Runtime.Events
{
	public readonly struct UnitCreatedEvent : IEvent
	{
		public Unit Unit { get; }

		public UnitCreatedEvent(Unit unit) => Unit = unit;

		public override string ToString() => $"[UnitCreated] {Unit.name}({Unit.Id}) at {Unit.position}";
	}

	public readonly struct UnitDestroyedEvent : IEvent
	{
		public Unit Unit { get; }
		public Vector2Int DeathPosition { get; }
		public string KillerUnitId { get; }

		public UnitDestroyedEvent(Unit unit, string killerUnitId = null)
		{
			Unit = unit;
			DeathPosition = unit.position;
			KillerUnitId = killerUnitId;
		}

		public override string ToString()
		{
			var killerInfo = string.IsNullOrEmpty(KillerUnitId) ? "自然死亡" : $"被 {KillerUnitId} 击杀";
			return $"[UnitDestroyed] {Unit.name}({Unit.Id}) at {DeathPosition}, {killerInfo}";
		}
	}
}
