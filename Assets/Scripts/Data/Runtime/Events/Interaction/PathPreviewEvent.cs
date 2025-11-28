using System.Collections.Generic;
using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct PathPreviewEvent : IEvent
	{
		public IReadOnlyList<Vector2Int> Path { get; }

		public int TotalCost { get; }

		public bool IsValid { get; }

		public string UnitId { get; }

		public PathPreviewEvent(
			IReadOnlyList<Vector2Int> path,
			int totalCost,
			bool isValid,
			string unitId)
		{
			Path = path ?? new List<Vector2Int>();
			TotalCost = totalCost;
			IsValid = isValid;
			UnitId = unitId;
		}

		/// <summary>
		/// Creates an event to hide the path preview.
		/// </summary>
		public static PathPreviewEvent Hide() =>
			new(new List<Vector2Int>(), 0, false, null);

		public override string ToString() =>
			$"[PathPreview] {Path.Count} cells, Cost:{TotalCost}, Valid:{IsValid}";
	}
}
