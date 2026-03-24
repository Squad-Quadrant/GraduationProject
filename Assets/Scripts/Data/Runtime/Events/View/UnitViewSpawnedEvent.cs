using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.View
{
	public readonly struct UnitViewSpawnedEvent : IEvent
	{
		public string UnitId { get; }
		public Transform Transform { get; }

		public UnitViewSpawnedEvent(string unitId, Transform transform)
		{
			UnitId = unitId;
			Transform = transform;
		}
	}
}
