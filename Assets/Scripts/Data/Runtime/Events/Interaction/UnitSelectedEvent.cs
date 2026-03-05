using System.Collections.Generic;
using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Interaction
{
	/// <summary>
	/// Published by InteractionStateMachine when a unit is selected.
	/// </summary>
	public readonly struct UnitSelectedEvent : IEvent
	{
		public string UnitId { get; }

		public Vector2Int Position { get; }

		public UnitSelectedEvent(string unitId, Vector2Int position)
		{
			UnitId = unitId;
			Position = position;
		}

		public override string ToString() => $"[UnitSelected] {UnitId} at {Position}";
	}
}
