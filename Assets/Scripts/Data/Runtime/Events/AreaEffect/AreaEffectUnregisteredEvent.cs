using System.Collections.Generic;
using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.AreaEffect
{
	public readonly struct AreaEffectUnregisteredEvent : IEvent
	{
		public string EffectId { get; }
		public IReadOnlyList<Vector2Int> Cells { get; }

		public AreaEffectUnregisteredEvent(string effectId, IReadOnlyList<Vector2Int> cells)
		{
			EffectId = effectId;
			Cells = cells;
		}

		public override string ToString() => $"[AreaEffectUnregistered] {EffectId} ({Cells.Count} cells)";
	}
}
