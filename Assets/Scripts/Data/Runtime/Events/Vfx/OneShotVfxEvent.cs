using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Vfx
{
	public readonly struct OneShotVfxEvent : IEvent
	{
		public GameObject Prefab { get; }
		public Vector2Int Cell { get; }

		public OneShotVfxEvent(GameObject prefab, Vector2Int cell)
		{
			Prefab = prefab;
			Cell = cell;
		}
	}
	public static class VfxBusExtensions
	{
		public static void PublishOneShotVfx(this IEventBus bus, GameObject prefab, Vector2Int cell)
		{
			if (!prefab) return;
			bus.Publish(new OneShotVfxEvent(prefab, cell));
		}
	}
}
