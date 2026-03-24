using Core.Events;

namespace Data.Runtime.Events.View
{
	// 为了fog of war渲染效果优化
	public readonly struct UnitViewDespawnedEvent : IEvent
	{
		public string UnitId { get; }

		public UnitViewDespawnedEvent(string unitId)
		{
			UnitId = unitId;
		}
	}
}
