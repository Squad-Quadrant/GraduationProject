using System.Collections.Generic;
using Core.Events;
using Systems.Map;

namespace Data.Runtime.Events.Map
{
	public readonly struct MapCellChangedEvent : IEvent
	{
		public MapCell Cell { get; }
		public IReadOnlyList<MapWall> Walls { get; }

		public MapCellChangedEvent(MapCell cell, IReadOnlyList<MapWall> walls)
		{
			Cell = cell;
			Walls = walls;
		}
	}
}
