using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Unit
{
	/// <summary>
	/// Triggered when a unit is destroyed in the game.
	/// </summary>
	public readonly struct UnitDestroyedEvent : IEvent
	{
		public Systems.Unit.Unit Unit { get; }
		public Vector2Int DeathPosition { get; }
		public string KillerUnitId { get; }

		public UnitDestroyedEvent(Systems.Unit.Unit unit, string killerUnitId = null)
		{
			Unit = unit;
			DeathPosition = unit.position;
			KillerUnitId = killerUnitId;
		}

		public override string ToString()
		{
			var killerInfo = string.IsNullOrEmpty(KillerUnitId) ? "自然死亡" : $"被 {KillerUnitId} 击杀";
			return $"[UnitDestroyed] {Unit.name}({Unit.id}) at {DeathPosition}, {killerInfo}";
		}
	}
}
