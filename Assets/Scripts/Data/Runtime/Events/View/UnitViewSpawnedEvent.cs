using Core.Events;
using Presentation.Unit;
using UnityEngine;

namespace Data.Runtime.Events.View
{
	public readonly struct UnitViewSpawnedEvent : IEvent
	{
		public string UnitId { get; }
		public UnitView View { get; }

		public UnitViewSpawnedEvent(string unitId, UnitView view)
		{
			UnitId = unitId;
			View = view;
		}
	}
}
