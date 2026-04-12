using System.Collections.Generic;
using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Vision
{
	// 现在这个事件只由VisionService发布
	public readonly struct VisionChangedEvent : IEvent
	{
		public readonly HashSet<Vector2Int> VisibleCells;
		public readonly string UnitId;

		public VisionChangedEvent(HashSet<Vector2Int> visibleCells, string unitId)
		{
			VisibleCells = visibleCells;
			UnitId = unitId;
		}
	}
}

