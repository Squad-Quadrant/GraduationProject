using System.Collections.Generic;
using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Interaction
{
	public enum ERangeType
	{
		Selection,
		Movement,
		Attack,
		Interact,
		AreaEffectPreview,
		AreaEffectOverlay,
		HoverRangePreview,
	}

	/// <summary>
	/// Published when the game needs to display a range on the map.
	/// </summary>
	public readonly struct RangeDisplayEvent : IEvent
	{
		public ERangeType RangeType { get; }

		public IReadOnlyList<Vector2Int> Cells { get; }

		public IReadOnlyDictionary<Vector2Int, int> CellCosts { get; }

		public Vector2Int? Origin { get; }

		public string SourceUnitId { get; }

		public Color AreaEffectColor { get; }

		public RangeDisplayEvent(
			ERangeType rangeType,
			IReadOnlyList<Vector2Int> cells,
			IReadOnlyDictionary<Vector2Int, int> cellCosts = null,
			Vector2Int? origin = null,
			string sourceUnitId = null,
			Color? areaEffectColor = null)
		{
			RangeType = rangeType;
			Cells = cells ?? new List<Vector2Int>();
			CellCosts = cellCosts;
			Origin = origin;
			SourceUnitId = sourceUnitId;
			AreaEffectColor = areaEffectColor ?? Color.white;
		}

		/// <summary>
		/// Creates an event to hide/clear a specific range type.
		/// </summary>
		public static RangeDisplayEvent Clear(ERangeType rangeType) => new(rangeType, new List<Vector2Int>());

		public override string ToString() => $"[RangeDisplay] {RangeType}: {Cells.Count} cells";
	}
}
