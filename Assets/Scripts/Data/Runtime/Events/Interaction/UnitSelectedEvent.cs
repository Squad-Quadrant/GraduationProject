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

		/// <summary>
		/// Actions available to this unit.
		/// UI uses this to show/hide action buttons.
		/// </summary>
		public IReadOnlyList<EActionType> AvailableActions { get; }

		public UnitSelectedEvent(string unitId, Vector2Int position, IReadOnlyList<EActionType> availableActions)
		{
			UnitId = unitId;
			Position = position;
			AvailableActions = availableActions;
		}

		public override string ToString() => $"[UnitSelected] {UnitId} at {Position}, Actions: {AvailableActions?.Count ?? 0}";
	}
}
