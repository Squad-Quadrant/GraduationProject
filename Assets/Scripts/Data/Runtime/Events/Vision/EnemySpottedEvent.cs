using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Vision
{
	// 标记一个敌人，代表其已经被发现了
	// 目前是手动触发，也可以在VisionService里自动触发，但是感觉现在不需要
	public readonly struct EnemySpottedEvent : IEvent
	{
		public readonly string UnitId;
		public readonly Vector2Int LastKnownPosition;

		public EnemySpottedEvent(string unitId, Vector2Int lastKnownPosition)
		{
			UnitId = unitId;
			LastKnownPosition = lastKnownPosition;
		}

		public override string ToString() => $"[EnemySpotted] Unit '{UnitId}' at {LastKnownPosition}";
	}
}
