using Core.Events;
using Systems.Map;
using UnityEngine;

namespace Data.Runtime.Events.Map
{
    public readonly struct MapViewInitEvent : IEvent
    {
        public MapData MapData { get; }

        public Sprite GroundSprite { get; }

        public MapViewInitEvent(MapData mapData, Sprite groundSprite)
        {
	        MapData = mapData;
	        GroundSprite = groundSprite;
        }
    }
}
