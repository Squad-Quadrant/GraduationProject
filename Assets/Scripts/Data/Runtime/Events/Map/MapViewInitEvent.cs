using Core.Events;
using Systems.Map;

namespace Data.Runtime.Events.Map
{
    public class MapViewInitEvent : IEvent
    {
        public MapData MapData { get; set; }
        public MapViewInitEvent(MapData mapData)
        {
            MapData = mapData;
        }
    }
}