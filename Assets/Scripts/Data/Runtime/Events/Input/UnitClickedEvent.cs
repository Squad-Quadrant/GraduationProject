using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Input
{
	/// <summary>
	/// This event is published instead of CellClickedEvent when a unit is clicked.
	/// </summary>
	public readonly struct UnitClickedEvent : IEvent
	{
		public string UnitId { get; }

		public Vector3 WorldPosition { get; }

		public UnitClickedEvent(string unitId, Vector3 worldPosition)
		{
			UnitId = unitId;
			WorldPosition = worldPosition;
		}

		public override string ToString() => $"[UnitClicked] Unit:{UnitId}";
	}
}
