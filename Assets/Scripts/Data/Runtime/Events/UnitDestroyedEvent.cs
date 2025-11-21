using Core.Events;
using Systems.Unit;
using UnityEngine;

namespace Data.Runtime.Events
{
	/// <summary>
	/// Triggered when a unit is destroyed in the game.
	/// </summary>
	public readonly struct UnitDestroyedEvent : IEvent
	{
		public Unit Unit { get; }
		public Vector2Int DeathPosition { get; }
		public string KillerUnitId { get; }

		public UnitDestroyedEvent(Unit unit, string killerUnitId = null)
		{
			Unit = unit;
			DeathPosition = unit.Position;
			KillerUnitId = killerUnitId;
		}

		public override string ToString()
		{
			var killerInfo = string.IsNullOrEmpty(KillerUnitId) ? "自然死亡" : $"被 {KillerUnitId} 击杀";
			return $"[UnitDestroyed] {Unit.Name}({Unit.Id}) at {DeathPosition}, {killerInfo}";
		}
	}
}
