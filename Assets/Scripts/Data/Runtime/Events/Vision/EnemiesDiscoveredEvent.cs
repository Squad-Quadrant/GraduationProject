using System.Collections.Generic;
using Core.Events;

namespace Data.Runtime.Events.Vision
{
	public readonly struct EnemiesDiscoveredEvent : IEvent
	{
		public readonly string MovingUnitId;

		public readonly IReadOnlyList<Systems.Unit.Unit> DiscoveredUnits;

		public EnemiesDiscoveredEvent(string movingUnitId, IReadOnlyList<Systems.Unit.Unit> discoveredUnits)
		{
			MovingUnitId = movingUnitId;
			DiscoveredUnits = discoveredUnits;
		}
	}
}
