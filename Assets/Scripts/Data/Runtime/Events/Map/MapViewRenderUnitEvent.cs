using System.Collections.Generic;
using Core.Events;
namespace Data.Runtime.Events.Map
{
    public class MapViewRenderUnitEvent : IEvent
    {
        public List<Systems.Unit.Unit> UnitsToRender { get; }
        public bool AutoGetUnits => UnitsToRender == null || UnitsToRender.Count == 0;
        public MapViewRenderUnitEvent(List<Systems.Unit.Unit> unitsToRender)
        {
            UnitsToRender = unitsToRender;
        }
    }
}