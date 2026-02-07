using System.Collections.Generic;
using Core.Events;
namespace Data.Runtime.Events.Map
{
    public class MapViewRenderUnitEvent : IEvent
    {
        public List<Systems.Unit.Unit> UnitsToRender { get; }
        public MapViewRenderUnitEvent(List<Systems.Unit.Unit> unitsToRender)
        {
            UnitsToRender = unitsToRender;
        }
    }
}