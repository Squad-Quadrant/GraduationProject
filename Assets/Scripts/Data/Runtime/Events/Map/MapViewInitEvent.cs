using Core.Events;
using Systems.Map;
namespace Data.Runtime.Events.Map
{
    public readonly struct MapViewInitEvent : IEvent
    {
        public MapData MapData { get; }

        public MapViewInitEvent(MapData mapData) => MapData = mapData;
    }
}
