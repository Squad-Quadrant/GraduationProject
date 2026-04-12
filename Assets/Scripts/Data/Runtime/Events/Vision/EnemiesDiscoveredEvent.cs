using System.Collections.Generic;
using Core.Events;

namespace Data.Runtime.Events.Vision
{
	// 在一个单位移动后，发现了新的敌人单位时触发的事件，包含所有新发现的敌人，主要用来触发动画
	// 被Spotted的敌人不会触发这个事件，但是因为需要一个“所有被发现的敌人”用来触发序列动画，所以这个事件仍然需要被保留
	public readonly struct EnemiesDiscoveredEvent : IEvent
	{
		public readonly string MovingUnitId;

		public readonly IReadOnlyList<Systems.Unit.Unit> DiscoveredUnits;

		public EnemiesDiscoveredEvent(string movingUnitId, IReadOnlyList<Systems.Unit.Unit> discoveredUnits)
		{
			MovingUnitId = movingUnitId;
			DiscoveredUnits = discoveredUnits;
		}
	}
}
